#!/bin/bash
# v1.0.0 发布自动重试：github.com 直连被网络阻断时每 60s 重试 push，
# push 成功后自动创建 GitHub Release（含 zip 资产）。
cd /e/code/new/ClassicalEconomics || exit 1

for i in $(seq 1 60); do
  if git push origin main >/dev/null 2>&1 && git push origin v1.0.0 >/dev/null 2>&1; then
    echo "PUSH_OK on attempt $i"
    break
  fi
  echo "attempt $i failed, waiting..."
  sleep 60
done

if git ls-remote --tags origin 2>/dev/null | grep -q 'refs/tags/v1.0.0'; then
  gh release create v1.0.0 \
    --title "v1.0.0 — Classical Economics" \
    --notes-file docs/release_notes_v1.0.0.md \
    "release/ClassicalEconomics-1.0.0.zip" 2>&1 | tail -3
  echo "RELEASE_DONE"
else
  echo "PUSH_STILL_BLOCKED"
fi
