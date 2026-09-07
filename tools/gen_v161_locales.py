# -*- coding: utf-8 -*-
"""v1.6.1: 15 new events x 6 languages. Idempotent (only fills missing keys)."""
import io, json, collections

EVENTS = collections.OrderedDict()
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, texts)

def q(*args): return args

# ===== 链尾（3）=====
ev('loan_recovery', 2,
 ('信誉重建', '信譽重建', 'Rebuilding Credit', 'Credit Repair', '信用回復', 'Kreditwiederherstellung'),
 ('债还清了，但{king}的信誉仍待重建——商人联盟试探性地送来一份新合同。', '債還清了，但{king}的信譽仍待重建——商人聯盟試探性地送來一份新合同。', 'The debt is settled, but {king}\'s credit is still to be rebuilt — the league tentatively sends a new contract.', 'Долг погашен, но репутацию {king} нужно восстановить — лига прислала пробный контракт.', '借金は返済されたが{king}の信用はまだ再建が要る——商人連盟が試しの契約を送ってきた。', 'Die Schuld ist beglichen, aber {king}\'s Kredit ist noch zu reparieren — die Liga sendet vorläufig einen neuen Vertrag.'),
 q('签新约', '簽新約', 'Sign a new pact', 'Подписать новый пакт', '新しい契約に調印', 'Neuen Pakt unterzeichnen'),
 q('以优惠利率再借一笔，滚动信用。', '以優惠利率再借一筆，滾動信用。', 'Borrow again at a favorable rate and roll the credit.', 'Взять второй заём по льготной ставке.', '優遇金利で再借入し信用を回転させる。', 'Erneut zu günstigem Zins leihen und den Kredit rollieren.'),
 q('新约签成，列国商界重新认可{kingdom}的信用。', '新約簽成，列國商界重新認可{kingdom}的信用。', 'The pact is signed; the merchant world re-acknowledges {kingdom}\'s credit.', 'Пакт подписан; мир торговли снова признаёт кредит {kingdom}.', '新契約が成立し商業界が{kingdom}の信用を再認識した。', 'Der Pakt ist unterschrieben; die Handelswelt erkennt {kingdom}\'s Kredit wieder an.'),
 q('婉拒', '婉拒', 'Decline', 'Отказаться', '辞退', 'Ablehnen'),
 q('不欠新债，踏踏实实攒钱。', '不欠新債，踏踏實實攢錢。', 'No new debts; save patiently.', 'Не брать новых долгов, копить.', '新たな借金をせず着実に貯める。', 'Keine neuen Schulden; geduldig sparen.'),
 q('婉拒新约，商界从此对{kingdom}三分敬意。', '婉拒新約，商界從此對{kingdom}三分敬意。', 'The refusal earns {kingdom} fresh respect in the merchant world.', 'Отказ приносит {kingdom} уважение торговцев.', '辞退して商人界の軽い敬意を得た。', 'Die Ablehnung bringt {kingdom} frischen Respekt im Handelswesen.')
)
ev('heir_legacy', 2,
 ('立储风波', '立儲風波', 'The Succession Storm', 'Буря престолонаследия', '立太子の騒動', 'Die Thronfolge-Sturm'),
 ('犯罪风波已平，但{king}不得不重新立储——朝野再次屏息。', '犯罪風波已平，但{king}不得不重新立儲——朝野再次屏息。', 'The dust settles, but {king} must name a new heir — the court holds its breath again.', 'Пыль улеглась, но {king} должен назвать нового наследника — двор снова затаил дыхание.', '騒動は収まったが{king}は再び跡継ぎを定めねばならない——廷が再び息を殺す。', 'Der Staub legt sich, aber {king} muss einen neuen Erben benennen — der Hof hält wieder den Atem an.'),
 q('册立长嗣', '冊立長嗣', 'Crown the elder', 'Короновать старшего', '長子を立てる', 'Den Älteren krönen'),
 q('依祖制立长，稳定人心但各方仍有龃龉。', '依祖制立長，穩定人心但各方仍有齟齬。', 'By tradition; stable, yet the factions still squabble.', 'По традиции; стабильно, но фракции всё ещё грызутся.', '祖制通り立太子で人心を安定させるが派閥の諍いが残る。', 'Nach Tradition; stabil, doch die Fraktionen zanken weiter.'),
 q('长嗣立储，国本渐固。', '長嗣立儲，國本漸固。', 'The elder heir is crowned; the foundation firms.', 'Старший коронован; фундамент крепнет.', '長子が立太子し国の基礎が固まった。', 'Der Ältere wird gekrönt; das Fundament festigt sich.'),
 q('废长立贤', '廢長立賢', 'Pick the worthier', 'Выбрать достойного', '賢君を立つ', 'Den Würdigeren wählen'),
 q('违背祖制立贤——争议与代价并存。', '違背祖制立賢——爭議與代價並存。', 'Against tradition — controversy and cost together.', 'Против традиции — скандал и цена вместе.', '祖制に背き賢君を立てる——論争と犠牲が伴う。', 'Gegen die Tradition — Kontroverse und Kosten zusammen.'),
 q('贤主登位，国政一新，但老臣怨声四起。', '賢主登位，國政一新，但老臣怨聲四起。', 'The worthy rules; governance renews, old ministers grumble.', 'Достойный правит; управление обновляется, старики ворчат.', '賢君が即位し政治が一新するが老臣の不満が渦巻く。', 'Der Würdige regiert; die Verwaltung erneuert sich, alte Minister murren.')
)
ev('defector_rebellion', 2,
 ('降将叛乱', '降將叛亂', 'The Defector\'s Rebellion', 'Мятеж перебежчика', '降将の反乱', 'Die Rebellion des Überläufers'),
 ('隐忍数年，降将终于举起反旗——当年放任的祸根长成了祸患。', '隱忍數年，降將終於舉起反旗——當年放任的禍根長成了禍患。', 'Enduring for years, the defector finally raises his banner — the tolerated seed grew into a plague.', 'Терпевший годы перебежчик поднимает знамя — прощённое семя стало бедой.', '数年耐えた降将がついに反旗を翻す——放任した禍根が禍患に育った。', 'Jahre des Duldens — der Überläufer hisst endlich sein Banner; der geduldete Same wurde zur Plage.'),
 q('御驾亲征', '御駕親征', 'Lead the army', 'Возглавить армию', '親征する', 'Die Armee anführen'),
 q('王室倾力平叛，杜绝尾大不掉。', '王室傾力平叛，杜絕尾大不掉。', 'The crown crushes the revolt at full strength.', 'Корона всей мощью давит мятеж.', '王室全力で討伐し尾大不掉を防ぐ。', 'Die Krone zertrümmert den Aufstand mit voller Kraft.'),
 q('亲征平叛，叛军授首，{kingdom}一战立威。', '親征平叛，叛軍授首，{kingdom}一戰立威。', 'The rebellion falls; {kingdom} wins fear in one battle.', 'Мятеж пал; {kingdom} заслужил страх одной битвой.', '親征で反乱を平定し{kingdom}が一戦で威を示した。', 'Der Aufstand fällt; {kingdom} gewinnt Furcht in einer Schlacht.'),
 q('怀柔分化', '懷柔分化', 'Divide & conciliate', 'Разделить и умиротворить', '懐柔で分化', 'Teilen & beschwichtigen'),
 q('许以高官厚禄分化其众——叛乱或可不战而平。', '許以高官厚祿分化其眾——叛亂或可不戰而平。', 'Buy off his followers with rank and gold.', 'Скупить его сторонников чинами и золотом.', '高官と俸禄で配下を分断——戦わずして鎮める。', 'Seine Anhänger mit Rang und Gold kaufen.'),
 q('分化奏效，叛军瓦解；{kingdom}耗财却保全元气。', '分化奏效，叛軍瓦解；{kingdom}耗財卻保全元氣。', 'The wedge works; the rebels dissolve — costly, but strength is kept.', 'Клин сработал; мятежники разошлись — дорого, но силы сохранены.', '分断が奏功し叛軍が瓦解——費用は掛かるが国力は保たれた。', 'Der Keil wirkt; die Rebellen lösen sich — teuer, aber die Kraft bleibt.')
)

# ===== 新事件（12）=====
ev('royal_heist', 2,
 ('宫库失窃', '宮庫失竊', 'The Vault Heist', 'Кража из казны', '宮庫の窃盗', 'Der Tresor-Raub'),
 ('一伙胆大包天的盗贼竟凿穿宫库，金砖不翼而飞。', '一夥膽大包天的盜賊竟鑿穿宮庫，金磚不翼而飛。', 'A brazen band tunneled into the vault — the ingots have vanished.', 'Дерзкая банда прорыла тоннель в казну — слитки исчезли.', '大胆な盗賊が宮庫を突き抜け金塊が消えた。', 'Eine dreiste Bande grub in den Tresor — die Barren sind verschwunden.'),
 q('全城缉盗', '全城緝盜', 'Hunt the thieves', 'Ловить воров', '全城で捜索', 'Diebe jagen'),
 q('重金悬赏，掘地三尺。', '重金懸賞，掘地三尺。', 'A heavy bounty; dig every hole.', 'Щедрая награда и обыск всего.', '高額の懸賞をかけ徹底捜索。', 'Ein hohes Kopfgeld; jedes Loch ausheben.'),
 q('盗贼落网，赃物追回大半。', '盜賊落網，贓物追回大半。', 'The thieves are caught; most loot returns.', 'Воры пойманы; большая часть добычи вернулась.', '盗賊が捕まり贓物の大半が戻った。', 'Die Diebe werden gefasst; die meisten Beute kehrt zurück.'),
 q('封锁消息', '封鎖消息', 'Hush it up', 'Замять', '情報封鎖', 'Vertuschen'),
 q('压住消息以免人心浮动——破财事小，失面子事大。', '壓住消息以免人心浮動——破財事小，失面子事大。', 'Suppress the news to keep calm — the loss is small, the face big.', 'Скрыть новость ради спокойствия — потеря мала, лицо велико.', '流言を抑え人心を静める——損失より面子が大事。', 'Die Nachricht unterdrücken, um Ruhe zu wahren — der Verlust ist klein, das Gesicht groß.'),
 q('消息虽被压下，宫中却从此多了风声鹤唳。', '消息雖被壓下，宮中卻從此多了風聲鶴唳。', 'The news is buried, but the court lives in alarm ever after.', 'Новость похоронена, но двор живёт в тревоге.', '情報は抑えられたが宮中の警戒が続く。', 'Die Nachricht ist begraben, aber der Hof lebt fortan in Alarm.')
)
ev('mint_shortage', 2,
 ('铸币短缺', '鑄幣短缺', 'Coin Shortage', 'Нехватка монет', '鋳貨不足', 'Münzknappheit'),
 ('铸币坊供不上新币，市面铜钱短缺，物物交换抬头。', '鑄幣坊供不上新幣，市面銅錢短缺，物物交換抬頭。', 'The mint cannot keep up; copper coins run short and barter returns.', 'Монетный двор не успевает; медных монет не хватает, возвращается бартер.', '鋳貨所が新貨を供給できず銅銭が不足し物々交換が戻る。', 'Die Münze hält nicht Schritt; Kupfermünzen werden knapp, Tauschhandel kehrt zurück.'),
 q('购铜扩铸', '購銅擴鑄', 'Buy copper & mint more', 'Купить медь и чеканить', '銅を買い増鋳', 'Kupfer kaufen und mehr prägen'),
 q('王室购铜扩铸，缓和市场。', '王室購銅擴鑄，緩和市場。', 'The crown buys copper and mints to ease the market.', 'Корона покупает медь и чеканит, облегчая рынок.', '王室が銅を買い増鋳して市場を緩和。', 'Die Krone kauft Kupfer und prägt, um den Markt zu lindern.'),
 q('新币入市，物价回稳。', '新幣入市，物價回穩。', 'Fresh coins enter; prices settle.', 'Свежие монеты входят; цены успокаиваются.', '新貨が流通し物価が安定した。', 'Frische Münzen kommen; Preise beruhigen sich.'),
 q('听之任之', '聽之任之', 'Let it be', 'Пусть как есть', '放置', 'So lassen'),
 q('不干预市场，让风俗自己适应。', '不干預市場，讓風俗自己適應。', 'No intervention; let custom adapt.', 'Не вмешиваться; пусть обычай адаптируется.', '市場に介入せず慣習に任せる。', 'Nicht eingreifen; der Brauch passt sich an.'),
 q('物物交换盛行，交易效率大减。', '物物交換盛行，交易效率大減。', 'Barter spreads; trade slows.', 'Бартер распространяется; торговля замедляется.', '物々交換が横行し交易効率が落ちた。', 'Tauschhandel breitet sich aus; der Handel verlangsamt sich.')
)
ev('volcano_eruption', 2,
 ('火山爆发', '火山爆發', 'Volcanic Eruption', 'Извержение вулкана', '火山噴火', 'Vulkanausbruch'),
 ('火山喷发，漫天火山灰遮蔽日光，附近村镇告急。', '火山噴發，漫天火山灰遮蔽日光，附近村鎮告急。', 'The volcano erupts; ash blots the sun; nearby villages cry for help.', 'Вулкан извергается; пепел закрывает солнце; деревни молят о помощи.', '火山が噴火し灰が日光を遮り近隣の村々が救助を求める。', 'Der Vulkan bricht aus; Asche verfinstert die Sonne; Dörfer rufen um Hilfe.'),
 q('疏散救灾', '疏散救災', 'Evacuate & aid', 'Эвакуировать и помочь', '避難と救済', 'Evakuieren & helfen'),
 q('开仓赈济，安置灾民。', '開倉賑濟，安置災民。', 'Open the granaries and shelter the displaced.', 'Открыть амбары и приютить беженцев.', '倉を開き被災者を収容する。', 'Die Kornkammern öffnen und die Vertriebenen unterbringen.'),
 q('火山灰下，{kingdom}的秩序没有崩塌。', '火山灰下，{kingdom}的秩序沒有崩塌。', 'Under the ash, {kingdom}\'s order holds.', 'Под пеплом порядок {kingdom} держится.', '灰の中でも{kingdom}の秩序は崩れなかった。', 'Unter der Asche hält die Ordnung von {kingdom}.'),
 q('闭关自守', '閉關自守', 'Seal the gates', 'Запереть ворота', '封城', 'Tore schließen'),
 q('听天由命，让灰烬自然落定。', '聽天由命，讓灰燼自然落定。', 'Leave fate to the ash.', 'Отдать пепел судьбе.', '灰が自然に落ち着くに任せる。', 'Das Schicksal der Asche überlassen.'),
 q('灰烬终落，流民却开始四邻乞讨。', '灰燼終落，流民卻開始四鄰乞討。', 'The ash settles, but the displaced beg at every gate.', 'Пепел осел, но беженцы побираются у всех ворот.', '灰は落ち着いたが難民が近隣に乞食する。', 'Die Asche sinkt, aber Vertriebene betteln an allen Toren.')
)
ev('plague_second', 2,
 ('瘟疫卷土', '瘟疫捲土', 'Plague Returns', 'Чума возвращается', '疫病再来', 'Die Pest kehrt zurück'),
 ('本以为平息了的瘟疫在邻市再度爆发，且势头更猛。', '本以為平息了的瘟疫在鄰市再度爆發，且勢頭更猛。', 'The plague thought quelled erupts again, fiercer than before.', 'Считавшаяся утихшей чума вспыхивает снова, злее прежнего.', '鎮まったと思われた疫病がさらに激しく再発した。', 'Die für überwunden gehaltene Pest bricht erneut aus — heftiger als zuvor.'),
 q('加固隔离', '加固隔離', 'Tighten quarantine', 'Усилить карантин', '隔離を強化', 'Quarantäne verschärfen'),
 q('再拨专款设医棚封锁疫区。', '再撥專款設醫棚封鎖疫區。', 'Fund new infirmaries and seal the zone.', 'Выделить на лазареты и закрыть зону.', '医棚をもうけ疫病地域を封鎖する。', 'Neue Pflegehäuser finanzieren und die Zone sperren.'),
 q('疫区被控，{kingdom}的公共卫生声誉反而上升。', '疫區被控，{kingdom}的公共衛生聲譽反而上升。', 'The zone is contained; public-health reputation rises.', 'Зона изолирована; репутация здравоохранения растёт.', '地域が封鎖され公衆衛生の評判が上がった。', 'Die Zone ist eingedämmt; der Ruf der öffentlichen Gesundheit steigt.'),
 q('听天由命', '聽天由命', 'Leave it to fate', 'На милость судьбы', '運に任せる', 'Dem Schicksal überlassen'),
 q('不增预算，让瘟疫自生自灭。', '不增預算，讓瘟疫自生自滅。', 'No new funds; let it burn itself out.', 'Без средств; пусть выгорит сама.', '予算を増やさず疫病の自滅を待つ。', 'Keine neuen Mittel; sich selbst ausbrennen lassen.'),
 q('瘟疫肆虐，{kingdom}的城市一片愁云惨雾。', '瘟疫肆虐，{kingdom}的城市一片愁雲慘霧。', 'The plague rages; gloom blankets {kingdom}\'s cities.', 'Чума бушует; уныние накрывает города {kingdom}.', '疫病が猛威を振るい{kingdom}の都市に暗雲。', 'Die Pest wütet; Düsternis liegt über den Städten.')
)
ev('cult_rise', 2,
 ('异端教派', '異端教派', 'A Cult Rises', 'Возникновение культа', '異端の教団', 'Ein Kult erhebt sich'),
 ('一个新教派宣称末日将至，信徒日众，寺庙香火鼎盛。', '一個新教派宣稱末日將至，信徒日眾，寺廟香火鼎盛。', 'A new sect preaches doom; believers multiply and temples glow.', 'Новая секта проповедует конец света; верующие множатся, храмы сияют.', '終末を説く新宗派が信者を増やし寺院の香火が盛ん。', 'Eine neue Sekte predigt den Untergang; Gläubige mehren sich, Tempel erglühen.'),
 q('承认教派', '承認教派', 'Acknowledge the sect', 'Признать секту', '教団を認める', 'Die Sekte anerkennen'),
 q('给予合法地位，换取教团捐赠与秩序。', '給予合法地位，換取教團捐贈與秩序。', 'Legalize it in exchange for donations and calm.', 'Легализовать в обмен на пожертвования и покой.', '合法化して寄付と秩序を引き換えに。', 'Legalisieren im Tausch gegen Spenden und Ruhe.'),
 q('教团献金入库，其教义也悄悄渗入政令。', '教團獻金入庫，其教義也悄悄滲入政令。', 'Their gold enters the vault; their creed seeps into the edicts.', 'Золото входит в казну; их догмы просачиваются в указы.', '教団の献金が庫に入り教義が政令へ滲む。', 'Ihr Gold fließt in den Tresor; ihr Credo sickert in die Erlasse.'),
 q('取缔教派', '取締教派', 'Outlaw the sect', 'Запретить секту', '教団を禁ずる', 'Die Sekte ächten'),
 q('查禁教团、拆毁神庙。', '查禁教團、拆毀神廟。', 'Proscribe it and tear down the temples.', 'Запретить и снести храмы.', '教団を禁止し寺院を破壊する。', 'Sie ächten und die Tempel niederreißen.'),
 q('教派被取缔，但信众转入地下暗流涌动。', '教派被取締，但信眾轉入地下暗流湧動。', 'Outlawed, but the believers go underground.', 'Запрещена, но верующие уходят в подполье.', '禁止されたが信者は地下に潜り暗流がうごめく。', 'Geächtet, aber die Gläubigen gehen in den Untergrund.')
)
ev('cult_aftermath', 2,
 ('教派余波', '教派餘波', 'Aftermath of the Cult', 'Последствия культа', '教団の余波', 'Nachwehen des Kults'),
 ('教派虽已定性，其教众却散落各地，如何善后？', '教派雖已定性，其教眾卻散落各地，如何善後？', 'The sect is judged, but its followers are scattered — how to recover?', 'Секта осуждена, но её последователи рассеяны — как восстанавливаться?', '教団は裁かれたが信者が各地に散らばる——如何に収拾するか。', 'Die Sekte ist verurteilt, aber ihre Anhänger sind verstreut — wie erholen?'),
 q('大赦归农', '大赦歸農', 'Amnesty them', 'Амнистия', '恩赦で農に帰す', 'Amnestie'),
 q('赦免归农，重编户籍。', '赦免歸農，重編戶籍。', 'Amnesty; re-register them as farmers.', 'Амнистия; переписать в земледельцы.', '恩赦し戸籍を再編する。', 'Amnestie; als Bauern neu registrieren.'),
 q('大赦之下，流散的教众终于回到田间。', '大赦之下，流散的教眾終於回到田間。', 'Under amnesty, the scattered believers return to the fields.', 'Под амнистией рассеянные вернулись к полям.', '恩赦で散った信者が田に戻った。', 'Unter Amnestie kehren die Verstreuten auf die Felder zurück.'),
 q('暗中监视', '暗中監視', 'Watch them', 'Следить', '密かに監視', 'Beobachten'),
 q('编入户籍但派密探按户盯梢。', '編入戶籍但派密探按戶盯梢。', 'Register them but shadow every household.', 'Переписать, но следить за каждым домом.', '戸籍に入れるが密探を張り付ける。', 'Registrieren, aber jeden Haushalt beschatten.'),
 q('监视网下，教派势力难以再生。', '監視網下，教派勢力難以再生。', 'Under the watch, the sect finds no new life.', 'Под надзором секта не возрождается.', '監視網の下で教団の勢力は再生できず。', 'Unter Beobachtung findet die Sekte kein neues Leben.')
)
ev('princess_betrothal', 2,
 ('公主和亲', '公主和親', 'The Princess Betrothal', 'Помолвка принцессы', '公主の縁組', 'Die Prinzessinnen-Verlobung'),
 ('邻国求娶{kingdom}一位公主，愿以重礼换取联姻之好。', '鄰國求娶{kingdom}一位公主，願以重禮換取聯姻之好。', 'A neighbor seeks the hand of a princess, offering rich gifts for the marriage bond.', 'Сосед просит руку принцессы, предлагая щедрые дары за союз.', '隣国が公主を娶りたく重礼を差し出して姻親を結ぼうとする。', 'Ein Nachbar wirbt um eine Prinzessin und bietet reiche Gaben für das Eheband.'),
 q('许婚和亲', '許婚和親', 'Betroth her', 'Обручить', '縁組を許す', 'Sie verloben'),
 q('十里红妆送嫁，两国从此结为姻亲。', '十里紅妝送嫁，兩國從此結為姻親。', 'A grand procession; the two courts become kin.', 'Пышный кортеж; два двора становятся роднёй.', '豪華な輿入れで両国が姻戚となる。', 'Ein prächtiger Zug; die zwei Höfe werden verwandt.'),
 q('公主出嫁，婚盟为{kingdom}赢得一个强援。', '公主出嫁，婚盟為{kingdom}贏得一個強援。', 'The marriage wins {kingdom} a powerful ally.', 'Брак даёт {kingdom} могущественного союзника.', '出嫁で{kingdom}が強力な支援を得た。', 'Die Ehe gewinnt {kingdom} einen mächtigen Verbündeten.'),
 q('婉拒求亲', '婉拒求親', 'Decline the suit', 'Отказать', '申し込みを辞退', 'Die Werbung ablehnen'),
 q('以公主年幼或体弱为由，婉拒求亲。', '以公主年幼或體弱為由，婉拒求親。', 'Plead youth or health and decline.', 'Сослаться на юность или здоровье.', '公主の若さや病弱を理由に辞退。', 'Auf Jugend oder Gesundheit plädieren und ablehnen.'),
 q('求亲被拒，邻国使节悻悻而归。', '求親被拒，鄰國使節悻悻而歸。', 'The suit refused; the envoy leaves chagrined.', 'Отказ; посол уходит досадливо.', '申し込みを断られ使節が悔しげに帰った。', 'Die Werbung abgelehnt; der Gesandte geht verärgert.')
)
ev('border_skirmish', 2,
 ('边境摩擦', '邊境摩擦', 'Border Skirmish', 'Пограничная стычка', '国境紛争', 'Grenzscharmützel'),
 ('边境哨所爆发冲突，双方各执一词，战火一触即发。', '邊境哨所爆發衝突，雙方各執一詞，戰火一觸即發。', 'A border outpost clashes; both sides blame each other; war looms.', 'Стычка на заставе; обе стороны винят друг друга; война на пороге.', '国境の哨所で衝突、双方が言い争い戦火が一触即発。', 'Ein Grenzposten kollidiert; beide geben einander die Schuld; der Krieg droht.'),
 q('克制忍让', '克制忍讓', 'Restrain', 'Сдержаться', '自制して譲る', 'Zurückhaltung'),
 q('斥责哨官，退兵一步，息事宁人。', '斥責哨官，退兵一步，息事寧人。', 'Rebuke the officer, pull back a step, calm it.', 'Упрекнуть офицера, отойти на шаг, успокоить.', '哨官を叱責し一歩下がって沈静化。', 'Den Offizier rügen, einen Schritt zurück, beruhigen.'),
 q('克制换来了邻国的口头致歉，事态暂平。', '克制換來了鄰國的口頭致歉，事態暫平。', 'Restraint earns a verbal apology; the matter cools.', 'Сдержанность приносит словесные извинения; дело остывает.', '自制が隣国の謝罪を生み事態が沈静化。', 'Zurückhaltung bringt eine mündliche Entschuldigung; die Sache kühlt ab.'),
 q('以牙还牙', '以牙還牙', 'Retaliate', 'Ответить ударом', '倍返し', 'Vergelten'),
 q('派兵搜查边境，誓要讨个说法。', '派兵搜查邊境，誓要討個說法。', 'Send troops to scour the border and demand justice.', 'Отправить войска прочесать границу.', '兵を派して国境を捜索し説明を求める。', 'Truppen entsenden, die Grenze zu durchsuchen und Genugtuung zu fordern.'),
 q('边境气氛骤紧，两国开始互相陈兵。', '邊境氣氛驟緊，兩國開始互相陳兵。', 'The border tenses; both sides mass troops.', 'Граница напрягается; обе стороны стягивают войска.', '国境が緊張し両国が兵を並べ始めた。', 'Die Grenze spannt sich; beide Seiten massieren Truppen.')
)
ev('war_escalation', 2,
 ('战争升级', '戰爭升級', 'War Escalation', 'Эскалация войны', '戦争拡大', 'Kriegseskalation'),
 ('摩擦未能平息，两国正式陈兵对峙，商旅纷纷改道。', '摩擦未能平息，兩國正式陳兵對峙，商旅紛紛改道。', 'The friction escalates; armies face off; caravans detour.', 'Трение нарастает; армии стоят друг против друга; караваны сворачивают.', '摩擦が収まらず両国が軍を並べ商旅が迂回する。', 'Die Reibung eskaliert; Armeen stehen sich gegenüber; Karawanen weichen aus.'),
 q('先发制人', '先發制人', 'Strike first', 'Ударить первым', '先手を打つ', 'Zuerst schlagen'),
 q('号令三军，抢先开战。', '號令三軍，搶先開戰。', 'Order the army and open the war.', 'Приказать армии и открыть войну.', '全軍に号令し先に開戦する。', 'Der Armee befehlen und den Krieg eröffnen.'),
 q('先发制人抢得先机，战局于{kingdom}有利。', '先發制人搶得先機，戰局於{kingdom}有利。', 'The first strike buys the initiative; the war favors {kingdom}.', 'Первый удар даёт инициативу; война на стороне {kingdom}.', '先手が機先を制し戦況が{kingdom}に有利。', 'Der erste Schlag verschafft Initiative; der Krieg begünstigt {kingdom}.'),
 q('边境谈判', '邊境談判', 'Negotiate', 'Переговоры', '国境交渉', 'Verhandeln'),
 q('遣使谈判，力求化干戈为玉帛。', '遣使談判，力求化干戈為玉帛。', 'Send envoys to beat swords into ploughshares.', 'Послать послов, чтобы перековать мечи на орала.', '使節を派遣し干戈を玉帛に変える。', 'Gesandte senden, um Schwerter zu Pflugscharen zu schmieden.'),
 q('谈判桌上双方各让一步，战争悬而未发。', '談判桌上雙方各讓一步，戰爭懸而未發。', 'Both yield a step at the table; the war stays unlit.', 'Оба уступают за столом; война не разгорается.', '交渉で双方が一歩譲り戦争は未発。', 'Beide weichen am Tisch zurück; der Krieg bleibt unentfacht.')
)
ev('armory_explosion', 2,
 ('军械库爆炸', '軍械庫爆炸', 'The Armory Blast', 'Взрыв арсенала', '兵器庫の爆発', 'Die Arsenal-Explosion'),
 ('军械库深夜剧烈爆炸，火光冲天，死伤惨重。', '軍械庫深夜劇烈爆炸，火光沖天，死傷慘重。', 'The armory erupts at night; flames and heavy casualties.', 'Арсенал взрывается ночью; пламя и большие потери.', '深夜に兵器庫が大爆発し炎が天を衝き死傷多数。', 'Das Arsenal explodiert nachts; Flammen und schwere Verluste.'),
 q('重建军械库', '重建軍械庫', 'Rebuild it', 'Отстроить', '兵器庫を再建', 'Wieder aufbauen'),
 q('拨专款重建，并加强守卫。', '撥專款重建，並加強守衛。', 'Fund a rebuild and strengthen the guard.', 'Выделить на отстройку и усилить охрану.', '専款で再建し警備を強化する。', 'Wiederaufbau finanzieren und die Wache verstärken.'),
 q('军械库重建完毕，守备森严。', '軍械庫重建完畢，守備森嚴。', 'The rebuilt armory stands guarded.', 'Отстроенный арсенал стоит под охраной.', '再建された兵器庫は厳重な警備。', 'Das wiederaufgebaute Arsenal steht bewacht.'),
 q('追责工匠', '追責工匠', 'Blame the artisans', 'Наказать мастеров', '工匠を問責', 'Die Handwerker belangen'),
 q('严查玩忽职守者，杀一儆百。', '嚴查玩忽職守者，殺一儆百。', 'Punish negligence harshly as a warning.', 'Сурово наказать за халатность в назидание.', '職務怠慢を厳しく追及し見せしめ。', 'Nachlässigkeit hart bestrafen als Warnung.'),
 q('工匠被下狱，军械之训自此严苛。', '工匠被下獄，軍械之訓自此嚴苛。', 'Artisans jailed; the ordinance of arms turns stern.', 'Мастера в тюрьме; воинский устав становится суров.', '工匠が下獄し軍械の規則が厳しくなる。', 'Handwerker eingekerkert; die Waffenordnung wird streng.')
)
ev('city_fire', 2,
 ('城市大火', '城市大火', 'The Great City Fire', 'Великий пожар', '都市大火', 'Der große Stadtbrand'),
 ('夜半火光冲天，半个城坊陷入火海。', '夜半火光沖天，半個城坊陷入火海。', 'A blaze at midnight engulfs half the city quarter.', 'Пожар в полночь охватывает половину квартала.', '真夜中の火光が半街区を火の海に。', 'Ein Feuer um Mitternacht verschlingt halb das Viertel.'),
 q('组织救火', '組織救火', 'Fight the fire', 'Тушить', '消火を組織', 'Das Feuer bekämpfen'),
 q('征民夫拆屋断火道，全力扑救。', '徵民夫拆屋斷火道，全力撲救。', 'Call the mob to raze a firebreak and fight.', 'Созвать народ разрушить разрыв и тушить.', '人夫を集め防火帯を切り全力で消火。', 'Den Pöbel rufen, eine Schneise zu reißen und zu löschen.'),
 q('火势被阻，大半城坊得以保全。', '火勢被阻，大半城坊得以保全。', 'The flames are stopped; most of the quarter survives.', 'Пламя остановлено; квартал большей частью спасён.', '火勢が食い止められ大半の街区が守られた。', 'Die Flammen sind gestoppt; das Viertel überlebt größtenteils.'),
 q('封城断火', '封城斷火', 'Seal & cut', 'Запереть и отрезать', '封城で火を断つ', 'Sperren & abtrennen'),
 q('封断火区，任其烧尽。', '封斷火區，任其燒盡。', 'Seal the zone and let it burn.', 'Закрыть зону и дать ей выгореть.', '火災地域を封鎖し燃え尽きるに任せる。', 'Die Zone sperren und ausbrennen lassen.'),
 q('大火烧了半夜才熄，焦尸枕藉。', '大火燒了半夜才熄，焦屍枕藉。', 'The blaze burns all night; charred bodies lie about.', 'Пожар бушует всю ночь; обугленные тела повсюду.', '火は半日燃え続け焦げた遺体が累々。', 'Das Feuer brennt die ganze Nacht; verkohlte Leichen liegen umher.')
)
ev('refugee_wave', 2,
 ('流民涌入', '流民湧入', 'A Wave of Refugees', 'Волна беженцев', '難民の流入', 'Eine Flut von Flüchtlingen'),
 ('邻国战乱，大批流民涌向{kingdom}边境，衣食无着。', '鄰國戰亂，大批流民湧向{kingdom}邊境，衣食無著。', 'Neighboring war drives refugees to {kingdom}\'s borders, without food or clothes.', 'Соседняя война гонит беженцев к границам {kingdom} без еды и одежды.', '隣国の戦乱で難民が{kingdom}の国境に衣食もなく押し寄せる。', 'Nachbarlicher Krieg treibt Flüchtlinge an die Grenzen von {kingdom}, ohne Essen und Kleider.'),
 q('开仓赈济', '開倉賑濟', 'Feed them', 'Кормить', '倉を開いて救済', 'Sie speisen'),
 q('设棚施粥，收容编籍。', '設棚施粥，收容編籍。', 'Set up shelters, distribute gruel, register them.', 'Открыть приюты, раздавать похлёбку, переписать.', '小屋を設け粥を施し戸籍に編入。', 'Unterkünfte aufbauen, Grütze verteilen, registrieren.'),
 q('流民落户，{kingdom}人口与人心同增。', '流民落戶，{kingdom}人口與人心同增。', 'The refugees settle; population and goodwill grow.', 'Беженцы осели; население и добрая воля растут.', '難民が定住し人口と人心が増えた。', 'Die Flüchtlinge siedeln sich an; Bevölkerung und Wohlwollen wachsen.'),
 q('驱赶出境', '驅趕出境', 'Drive them off', 'Прогнать', '追い返す', 'Sie vertreiben'),
 q('闭门不纳，押送出境。', '閉門不納，押送出境。', 'Close the gates and escort them out.', 'Закрыть ворота и выпроводить.', '門を閉じ境外へ移送する。', 'Die Tore schließen und sie hinausbegleiten.'),
 q('流民被驱散，怨声随去；邻国的年轻人口却再难归来。', '流民被驅散，怨聲隨去；鄰國的年輕人口卻再難歸來。', 'The refugees scatter; the neighbor\'s young population never returns.', 'Беженцы рассеялись; молодое население соседа не вернётся.', '難民は散らされ隣国の若い人口は戻らない。', 'Die Flüchtlinge zerstreuen sich; die junge Bevölkerung des Nachbarn kehrt nie zurück.')
)
ev('ally_betrayal', 2,
 ('盟友背叛', '盟友背叛', 'An Ally Betrays', 'Предательство союзника', '同盟の裏切り', 'Ein Verbündeter verrät'),
 ('曾经的盟国暗中与{kingdom}的宿敌勾连，秘密协议被截获。', '曾經的盟國暗中與{kingdom}的宿敵勾連，秘密協議被截獲。', 'A former ally secretly links with {kingdom}\'s enemy; the pact is intercepted.', 'Бывший союзник тайно связался с врагом {kingdom}; договор перехвачен.', 'かつての同盟国が宿敵と結託し密約が盗まれた。', 'Ein früherer Verbündeter verband sich heimlich mit dem Feind; der Pakt wurde abgefangen.'),
 q('公开断交', '公開斷交', 'Sever ties openly', 'Разорвать открыто', '公然と断交', 'Offen die Beziehungen brechen'),
 q('斥其背信，正式断交。', '斥其背信，正式斷交。', 'Denounce the treachery and sever.', 'Обличить коварство и разорвать.', '背信を責め正式に断交する。', 'Die Treulosigkeit anprangern und abbrechen.'),
 q('断交公告震动列国，{kingdom}的强硬赢得忧惧与敬意。', '斷交公告震動列國，{kingdom}的強硬贏得憂懼與敬意。', 'The severance rocks the courts; {kingdom}\'s firmness wins fear and respect.', 'Разрыв сотрясает дворы; твёрдость {kingdom} вызывает страх и уважение.', '断交が列国を震わせ{kingdom}の強硬さが畏敬を得る。', 'Der Bruch erschüttert die Höfe; die Härte von {kingdom} gewinnt Furcht und Respekt.'),
 q('隐忍不发', '隱忍不發', 'Bide it', 'Смолчать', '忍んで隠す', 'Schweigen'),
 q('不动声色，暗中收紧同盟条款作为筹码。', '不動聲色，暗中收緊同盟條款作為籌碼。', 'Say nothing; quietly hedge the alliance terms as leverage.', 'Молчать; тихо ужесточить условия как козырь.', '動かずに条約の条件を密かに見直す。', 'Nichts sagen; die Bedingungen still als Hebel verschärfen.'),
 q('隐忍让{kingdom}暂时稳住了局势，但信任之伤难以愈合。', '隱忍讓{kingdom}暫時穩住了局勢，但信任之傷難以癒合。', 'Restraint steadies the situation, but the wound of trust does not heal.', 'Сдержанность стабилизирует, но рана доверия не заживает.', '隠忍が局面を保つが信頼の傷は癒えない。', 'Zurückhaltung stabilisiert, aber die Vertrauenswunde heilt nicht.')
)
ev('secret_negotiation', 2,
 ('秘密谈判', '秘密談判', 'Secret Negotiation', 'Тайные переговоры', '秘密交渉', 'Geheime Verhandlung'),
 ('一个第三国遣密使来见{king}，提议与{kingdom}暗中结好，条件优厚。', '一個第三國遣密使來見{king}，提議與{kingdom}暗中結好，條件優厚。', 'A third power sends a secret envoy to {king}, offering a clandestine friendship on rich terms.', 'Третья держава шлёт тайного посланца к {king}, предлагая скрытую дружбу на выгодных условиях.', '第三国が密使を{king}に送り有利な条件で暗中の親善を提案。', 'Eine dritte Macht sendet einen geheimen Boten zu {king} mit reichen Bedingungen für heimliche Freundschaft.'),
 q('接受密约', '接受密約', 'Accept', 'Принять', '密約を受ける', 'Annehmen'),
 q('暗中答应，换取利益与情报。', '暗中答應，換取利益與情報。', 'Agree secretly for gains and intelligence.', 'Согласиться тайно ради выгод и разведки.', '密かに承諾し利益と情報を換取する。', 'Heimlich zustimmen für Gewinn und Nachrichten.'),
 q('密约生效，{kingdom}在暗处多了一个朋友。', '密約生效，{kingdom}在暗處多了一個朋友。', 'The pact holds; {kingdom} gains a friend in the shadows.', 'Договор действует; {kingdom} получает друга в тени.', '密約が効力を持ち{kingdom}が影に友を得た。', 'Der Pakt gilt; {kingdom} gewinnt einen Freund im Schatten.'),
 q('断然拒绝', '斷然拒絕', 'Refuse', 'Отказаться', '断固拒否', 'Ablehnen'),
 q('不与暗中势力做交易，正色拒之。', '不與暗中勢力做交易，正色拒之。', 'No deals with shadow powers; refuse sternly.', 'Никаких сделок с тенью; сурово отказать.', '闇の勢力と取引せず厳然と拒否。', 'Keine Geschäfte mit Schattenmächten; streng ablehnen.'),
 q('密使被逐，第三国从此与{kingdom}结怨。', '密使被逐，第三國從此與{kingdom}結怨。', 'The envoy is expelled; the third power bears a grudge.', 'Посол изгнан; третья держава затаила обиду.', '密使が追放され第三国が{kingdom}に恨む。', 'Der Bote wird vertrieben; die dritte Macht trägt einen Groll.')
)

# ===== 六語マージ =====
LANGS = {'ch': 0, 'zh_tw': 1, 'en': 2, 'ru': 3, 'ja': 4, 'de': 5}
for lang, idx in LANGS.items():
    p = 'Locales/%s.json' % lang
    d = json.load(io.open(p, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
    added = 0
    for eid, (opts, title, desc, texts) in EVENTS.items():
        kv = {
            'ev_%s' % eid: title[idx],
            'ev_%s_desc' % eid: desc[idx],
        }
        for i in range(opts):
            n = texts[i * 3][idx]      # 每选项 3 行，每行 6 语言
            dd = texts[i * 3 + 1][idx]
            r = texts[i * 3 + 2][idx]
            kv['ev_%s_opt%d' % (eid, i+1)] = n
            kv['ev_%s_opt%d_desc' % (eid, i+1)] = dd
            kv['ev_%s_res%d' % (eid, i+1)] = r
        for k, v in kv.items():
            if k not in d:
                d[k] = v; added += 1
    json.dump(d, io.open(p, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
    io.open(p, 'a', encoding='utf-8').write('\n')
    print(lang, 'added', added, 'total', len(d))
