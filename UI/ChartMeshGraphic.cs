using UnityEngine;
using UnityEngine.UI;

namespace EconomyMod.UI
{
    /// <summary>
    /// 顶点渲染折线图组件：继承 MaskableGraphic，在 OnPopulateMesh 中直接生成 UI 几何。
    /// 对比纹理方案：无 Texture2D 上传、无像素采样 → 任意 DPI / 分辨率 / Canvas 缩放下保持锐利；
    /// 半透明色带 / 渐变免费；顶点缓冲由 VertexHelper 复用，数据未变时零重建。
    /// 组件约定：RectTransform pivot=(0,0)，本地坐标原点在图表左下角（与刻度 Text 对齐）。
    /// 绘制内容（按序）：经济阶段色带 → 水平网格 → 参考虚线 → 主系列面积填充 → 各系列折线 → 末端圆点。
    /// </summary>
    public class ChartMeshGraphic : MaskableGraphic
    {
        // ===== 数据（内部深拷贝，外部复用缓冲安全）=====
        private float[][] _vals;      // [seriesCount][count]，NaN=断裂（不在榜）
        private Color[] _colors;      // 各系列颜色
        private Color[] _areaColors;  // 各系列面积填充色（仅主系列使用）
        private int[] _phases;        // 每点经济阶段
        private int _count;
        private int _seriesCount = 1;
        private float _vmin;
        private float _vmax = 1f;
        private bool _drawArea;       // 主系列面积填充（GDP）
        private bool _drawRefs;       // 参考虚线（Gini）
        private float _refHigh, _refLow;

        // ===== 签名 gating（数据未变不重建）=====
        private int _lastSig = int.MinValue;

        // ===== 绘制参数（公共，供 HUD 初始化时设置）=====
        public float yAxisWidth = 44f;
        public float margin = 4f;
        public int gridLines = 4;
        public Color gridColor = new Color(0.35f, 0.35f, 0.4f, 0.45f);
        public Color refHighColor = new Color(1f, 0.35f, 0.3f, 0.85f);
        public Color refLowColor = new Color(0.4f, 0.9f, 0.4f, 0.85f);
        public float dashLen = 6f;
        public float dashGap = 4f;
        public float thickness = 2f;
        public float pointRadius = 2.5f;
        public bool softEdge = true;   // 1px 亮芯 + 2px 半透明外描边（抗锯齿观感）

        // ===== 每帧布局缓存（OnPopulateMesh 时更新）=====
        private float _rectW, _rectH, _plotX0, _plotW, _plotH, _range;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_count < 2 || _vals == null) return;

            _rectW = rectTransform.rect.width;
            _rectH = rectTransform.rect.height;
            if (_rectW < 8f || _rectH < 8f) return;

            _plotX0 = yAxisWidth + margin;
            _plotW = _rectW - _plotX0 - margin;
            _plotH = _rectH - margin * 2f;
            if (_plotW <= 0f || _plotH <= 0f) return;

            _range = _vmax - _vmin;
            if (_range <= 0f) _range = 1f;

            DrawBands(vh);
            DrawGrid(vh);
            if (_drawRefs) DrawDashRefs(vh);
            if (_drawArea) DrawArea(vh);
            for (int s = 0; s < _seriesCount; s++) DrawSeries(vh, s);
        }

        /// <summary>设置图表数据（内部深拷贝）。vals[s][i] 第 s 系列第 i 点；NaN=该点不在榜。</summary>
        public void SetChartData(float[][] vals, Color[] colors, Color[] areaColors, int[] phases,
            int seriesCount, float vmin, float vmax, bool drawArea, bool drawRefs,
            float refHigh, float refLow)
        {
            if (vals == null || vals.Length == 0 || vals[0] == null || vals[0].Length < 2) return;

            int n = vals[0].Length;
            int sig = ComputeSig(vals, colors, phases, seriesCount, vmin, vmax,
                drawArea, drawRefs, refHigh, refLow);
            if (sig == _lastSig) return;

            _seriesCount = Mathf.Max(1, seriesCount);
            _vals = new float[_seriesCount][];
            for (int s = 0; s < _seriesCount; s++)
            {
                float[] src = s < vals.Length ? vals[s] : null;
                _vals[s] = new float[n];
                for (int i = 0; i < n; i++)
                    _vals[s][i] = src != null && i < src.Length ? src[i] : float.NaN;
            }
            _colors = new Color[_seriesCount];
            for (int s = 0; s < _seriesCount; s++)
                _colors[s] = colors != null && s < colors.Length ? colors[s] : Color.white;
            _areaColors = areaColors;
            _phases = new int[n];
            for (int i = 0; i < n; i++)
                _phases[i] = phases != null && i < phases.Length ? phases[i] : 0;
            _count = n;
            _vmin = vmin;
            _vmax = vmax > vmin ? vmax : vmin + 1f;
            _drawArea = drawArea;
            _drawRefs = drawRefs;
            _refHigh = refHigh;
            _refLow = refLow;
            _lastSig = sig;

            SetVerticesDirty();
        }

        /// <summary>重置为仅带空数据的可用状态（保留绘制参数，清空系列）。</summary>
        public void ClearChart()
        {
            _vals = null;
            _count = 0;
            _seriesCount = 1;
            _lastSig = int.MinValue;
            SetVerticesDirty();
        }

        private static int ComputeSig(float[][] vals, Color[] colors, int[] phases,
            int seriesCount, float vmin, float vmax, bool drawArea, bool drawRefs,
            float refHigh, float refLow)
        {
            int n = vals[0].Length;
            int h = 17;
            for (int s = 0; s < seriesCount; s++)
            {
                float[] arr = s < vals.Length ? vals[s] : null;
                if (arr == null) continue;
                h = h * 31 + (int)arr[0];
                for (int i = n - 1; i >= 0; i--)
                {
                    if (!float.IsNaN(arr[i])) { h = h * 31 + (int)arr[i]; break; }
                }
                h = h * 31 + (colors != null && s < colors.Length ? colors[s].GetHashCode() : 0);
            }
            for (int i = 0; i < n; i++)
                h = h * 31 + (phases != null && i < phases.Length ? phases[i] : 0);
            h = h * 31 + (int)vmin + (int)vmax;
            h = h * 31 + seriesCount;
            h = h * 31 + (drawArea ? 1 : 0) + (drawRefs ? 2 : 0);
            h = h * 31 + (int)refHigh + (int)refLow;
            return h;
        }

        // ===== 绘制：阶段色带 =====
        private void DrawBands(VertexHelper vh)
        {
            for (int i = 0; i < _count - 1; i++)
            {
                Color band = PhaseBand(_phases[i]);
                if (band.a <= 0.001f) continue;
                float x0 = XAt(i), x1 = XAt(i + 1);
                AddQuad(vh,
                    new Vector2(x0, 0f), new Vector2(x1, 0f),
                    new Vector2(x1, _rectH), new Vector2(x0, _rectH), band);
            }
        }

        // ===== 绘制：水平网格线 =====
        private void DrawGrid(VertexHelper vh)
        {
            for (int gi = 0; gi <= gridLines; gi++)
            {
                float y = YAt(_vmin + _range * gi / (float)gridLines);
                AddLineQuad(vh,
                    new Vector2(_plotX0, y), new Vector2(_rectW - margin, y),
                    1f, gridColor);
            }
        }

        // ===== 绘制：参考虚线（危险/健康）=====
        private void DrawDashRefs(VertexHelper vh)
        {
            DrawDashH(vh, _refHigh, refHighColor);
            DrawDashH(vh, _refLow, refLowColor);
        }

        private void DrawDashH(VertexHelper vh, float val, Color c)
        {
            float y = YAt(val);
            float len = dashLen + dashGap;
            if (len <= 0f) return;
            for (float x = _plotX0; x < _rectW - margin; x += len)
            {
                float e = x + dashLen;
                if (e > _rectW - margin) e = _rectW - margin;
                if (e - x > 0.5f)
                    AddLineQuad(vh, new Vector2(x, y), new Vector2(e, y), 1.5f, c);
            }
        }

        // ===== 绘制：主系列面积填充 =====
        private void DrawArea(VertexHelper vh)
        {
            float[] arr = _vals[0];
            Color fill = _areaColors != null && _areaColors.Length > 0 ? _areaColors[0] : _colors[0];
            fill.a = Mathf.Clamp01(fill.a * 0.5f);
            float baseY = YAt(_vmin);
            for (int i = 0; i < _count - 1; i++)
            {
                if (float.IsNaN(arr[i]) || float.IsNaN(arr[i + 1])) continue;
                AddQuad(vh,
                    new Vector2(XAt(i), baseY), new Vector2(XAt(i), YAt(arr[i])),
                    new Vector2(XAt(i + 1), YAt(arr[i + 1])), new Vector2(XAt(i + 1), baseY),
                    fill);
            }
        }

        // ===== 绘制：单系列折线 + 末端圆点 =====
        private void DrawSeries(VertexHelper vh, int s)
        {
            float[] arr = _vals[s];
            Color col = _colors[s];

            // 软边：半透明宽线铺底（抗锯齿观感）
            if (softEdge)
            {
                Color soft = col;
                soft.a = col.a * 0.25f;
                for (int i = 0; i < _count - 1; i++)
                {
                    if (float.IsNaN(arr[i]) || float.IsNaN(arr[i + 1])) continue;
                    AddLineQuad(vh,
                        new Vector2(XAt(i), YAt(arr[i])), new Vector2(XAt(i + 1), YAt(arr[i + 1])),
                        thickness + 2f, soft);
                }
            }
            // 实芯线
            for (int i = 0; i < _count - 1; i++)
            {
                if (float.IsNaN(arr[i]) || float.IsNaN(arr[i + 1])) continue;
                AddLineQuad(vh,
                    new Vector2(XAt(i), YAt(arr[i])), new Vector2(XAt(i + 1), YAt(arr[i + 1])),
                    thickness, col);
            }
            // 末端圆点（最后一个有效点）
            for (int i = _count - 1; i >= 0; i--)
            {
                if (float.IsNaN(arr[i])) continue;
                AddPoint(vh, new Vector2(XAt(i), YAt(arr[i])), pointRadius, col);
                break;
            }
        }

        // ===== 坐标转换（本地坐标，pivot=(0,0)，y 向上）=====
        private float XAt(int i) => _plotX0 + _plotW * i / (float)(_count - 1);
        private float YAt(float v) => margin + (v - _vmin) / _range * _plotH;

        // ===== 顶点辅助 =====
        private static void AddVert(VertexHelper vh, Vector2 p, Color c)
        {
            vh.AddVert(new Vector3(p.x, p.y, 0f), c, Vector2.zero);
        }

        private static void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color col)
        {
            int i = vh.currentVertCount;
            AddVert(vh, a, col); AddVert(vh, b, col); AddVert(vh, c, col); AddVert(vh, d, col);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        /// <summary>线段四边形：沿线段方向法线扩展厚度。</summary>
        private static void AddLineQuad(VertexHelper vh, Vector2 a, Vector2 b, float thick, Color c)
        {
            Vector2 d = b - a;
            float len = d.magnitude;
            if (len < 0.001f) return;
            Vector2 n = new Vector2(-d.y, d.x) / len * (thick * 0.5f);
            int i = vh.currentVertCount;
            AddVert(vh, a - n, c); AddVert(vh, a + n, c);
            AddVert(vh, b + n, c); AddVert(vh, b - n, c);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        /// <summary>圆点（三角扇 + 中心顶点）。</summary>
        private static void AddPoint(VertexHelper vh, Vector2 center, float radius, Color c)
        {
            const int seg = 12;
            int baseI = vh.currentVertCount;
            AddVert(vh, center, c);
            for (int k = 0; k <= seg; k++)
            {
                float ang = Mathf.PI * 2f * k / seg;
                AddVert(vh, center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius, c);
            }
            for (int k = 0; k < seg; k++)
                vh.AddTriangle(baseI, baseI + 1 + k, baseI + 2 + k);
        }

        /// <summary>经济阶段背景色带（半透明，与纹理版一致）。</summary>
        private static Color PhaseBand(int phase)
        {
            switch (phase)
            {
                case 0: return new Color(0.1f, 0.5f, 0.2f, 0.20f);  // Boom
                case 1: return new Color(0.5f, 0.35f, 0.1f, 0.16f); // Recession
                case 2: return new Color(0.5f, 0.12f, 0.1f, 0.22f); // Depression
                case 3: return new Color(0.1f, 0.3f, 0.5f, 0.16f);  // Recovery
                default: return new Color(0f, 0f, 0f, 0f);
            }
        }
    }
}
