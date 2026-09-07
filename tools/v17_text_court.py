# -*- coding: utf-8 -*-
"""v1.7.0 court 文案（六语）：40 个宫廷权谋事件（全 onlyPlayer）。"""
EVENTS = {}
def ev(eid, opts, title, desc, *texts):
    EVENTS[eid] = (opts, title, desc, list(texts))
def q(*args): return args

# ===== 宫闱之争（court_undercurrent 变体组，三选一互斥）=====
ev('duke_sword', 2,
 ('公爵佩剑', '公爵佩劍', 'The Duke\'s Sword', 'Меч герцога', '公爵の剣', 'Das Schwert des Herzogs'),
 ('寿宴之上，公爵竟佩剑入宫，剑鸣铮然，百官失色——{king}如何裁决？', '壽宴之上，公爵竟佩劍入宮，劍鳴錚然，百官失色——{king}如何裁決？', 'The duke comes to the feast with his sword; it rings aloud and all the courtiers pale. What shall {king} do?', 'Герцог явился на пир с мечом — он звенит, сановники бледнеют. Как поступит {king}?', '寿宴に公爵が剣を帯びて入宮——剣鳴り響き、百官の色が変わる。{king}は如何に裁くか。', 'Der Herzog kommt mit dem Schwert zum Fest; es klingt laut, alle Höflinge erbleichen. Was tut {king}?'),
 q('当庭夺剑', '當庭奪劍', 'Seize the sword', 'Отобрать меч', '剣を奪う', 'Das Schwert entreißen'),
 q('当庭夺剑削其护卫，杀一儆百。', '當庭奪劍削其護衛，殺一儆百。', 'Take the sword, strip his guards — a warning to all.', 'Отобрать меч и распустить его стражу — в назидание всем.', '廷上で剣を奪い護衛を削る——見せしめだ。', 'Das Schwert nehmen und seine Wachen entlassen — als Warnung für alle.'),
 q('公爵当众受辱，旧部哗然，宫闱自此暗流涌动。', '公爵當眾受辱，舊部譁然，宮闈自此暗流湧動。', 'The duke is shamed before all; his retainers mutter; undercurrents stir in the palace.', 'Герцог опозорен перед всеми; его вассалы ропщут, во дворце зреет смута.', '公爵は衆目の前で辱められ、旧臣らが騒然——宮闈に暗流が渦巻く。', 'Der Herzog ist vor allen gedemütigt; sein Gefolge murrt, im Palast gärt es.'),
 q('抚剑赐宴', '撫劍賜宴', 'Praise the sword', 'Похвалить меч', '剣を撫で賜宴', 'Das Schwert loben'),
 q('亲抚剑柄，笑言功高当赏。', '親撫劍柄，笑言功高當賞。', 'Grip the hilt yourself and smile: such merit deserves reward.', 'Коснуться рукояти с улыбкой: заслуги достойны награды.', '柄に手を添え、功は賞すべきと笑って言う。', 'Den Knauf ergreifen und lächeln: solches Verdienst verdient Lohn.'),
 q('公爵叩首谢恩，列国亦传{king}宽厚之名。', '公爵叩首謝恩，列國亦傳{king}寬厚之名。', 'The duke kowtows in gratitude; even foreign courts speak of {king}\'s magnanimity.', 'Герцог кланяется в благодарности; и чужие дворы говорят о великодушии {king}.', '公爵が頭を下げて謝す——列国も{king}の寛容を噂する。', 'Der Herzog dankt mit tiefer Verbeugung; selbst fremde Höfe rühmen {king}\'s Großmut.')
)
ev('palace_lady', 2,
 ('椒房贵戚', '椒房貴戚', 'The Bedchamber Kin', 'Родня из-за занавеса', '椒房の貴戚', 'Die Verwandten der Gemahlin'),
 ('新入宫的妃嫔竟是{king}的远亲，外戚之名已传遍市井，言官多以为忧。', '新入宮的妃嬪竟是{king}的遠親，外戚之名已傳遍市井，言官多以為憂。', 'The new concubine turns out to be {king}\'s distant kin; all the city buzzes of consort relatives, and the censors are much alarmed.', 'Новая наложница — дальняя родня {king}; о родне при дворе судачит весь город, и цензоры встревожены.', '新しく入宮した妃が{king}の遠縁——外戚の名が街に響き、言官の多くが憂える。', 'Die neue Gemahlin ist {king}\'s entfernte Verwandte; die Stadt redet von der Sippe, und die Zensoren sind beunruhigt.'),
 q('册封恩赏', '冊封恩賞', 'Ennoble and endow', 'Пожаловать с почестями', '冊封し恩賞', 'Erheben und beschenken'),
 q('依礼册封，取其贤良亲族入仕。', '依禮冊封，取其賢良親族入仕。', 'Ennoble her by rite and take the worthier kin into office.', 'Пожаловать по обряду и взять достойных родичей на службу.', '礼に依り冊封し、賢い親族を登用する。', 'Nach Sitte erheben und die tüchtigeren Verwandten in Amt nehmen.'),
 q('外戚感恩，列国皆赞{king}用人以贤，不以亲疏论功。', '外戚感恩，列國皆讚{king}用人以賢，不以親疏論功。', 'The kin are grateful; foreign courts praise {king}\'s choice of worth, which weighs merit over blood.', 'Родня благодарна; чужие дворы хвалят выбор {king} по достоинству, а не по крови.', '外戚が恩に感じ、列国も{king}の人材登用を讚える——親疎で功を論じず。', 'Die Sippe dankt; fremde Höfe loben {king}\'s Wahl nach Verdienst, nicht nach Blut.'),
 q('斥退宫闱', '斥退宮闈', 'Banish the lady', 'Выдворить из дворца', '宮闈を斥ける', 'Die Dame verstoßen'),
 q('斥其有干政之嫌，勒退其族。', '斥其有干政之嫌，勒退其族。', 'Blame her for dabbling in rule and send her kin away.', 'Обвинить в вмешательстве в правление и удалить её род.', '政に干す疑いを叱り、一族を追い退ける。', 'Sie der Einmischung in die Herrschaft zeihen und die Sippe verweisen.'),
 q('妃嫔泣血，外戚怨望，后宫风声鹤唳。', '妃嬪泣血，外戚怨望，後宮風聲鶴唳。', 'The lady weeps; her kin nurse grudges; the harem trembles with rumor.', 'Наложница рыдает, родня затаила обиду; гарем дрожит от слухов.', '妃が血を吐く思いで泣き、外戚は怨みを抱く——後宮に風声鶴唳。', 'Die Dame weint bitterlich; ihre Sippe grollt; der Harem bebt vor Gerüchten.')
)
ev('eunuch_power', 2,
 ('阉宦弄权', '閹宦弄權', 'The Eunuch\'s Clout', 'Власть евнуха', '宦官の権勢', 'Die Macht der Eunuchen'),
 ('宦官首领权倾内廷，{king}的诏令亦须经他之手，朝臣敢怒不敢言。', '宦官首領權傾內廷，{king}的詔令亦須經他之手，朝臣敢怒不敢言。', 'The eunuch chief rules the inner court; even {king}\'s edicts pass through his hands, and officials seethe in silence.', 'Глава евнухов вершит внутренним двором; даже указы {king} проходят через его руки, и сановники молча кипят.', '宦官の頭領が内廷を牛耳り、{king}の詔もその手を通る——臣らは怒りつつも言えぬ。', 'Der Eunuchenmeister regiert den Innenhof; selbst {king}\'s Erlasse gehen durch seine Hände, und die Beamten kochen still.'),
 q('金帛买安', '金帛買安', 'Buy his peace', 'Купить тишину', '金帛で買う', 'Ruhe erkaufen'),
 q('以重金绢帛笼络，换他约束阉党。', '以重金絹帛籠絡，換他約束閹黨。', 'Bribe him with gold and silk so he keeps his eunuchs in check.', 'Задарить золотом и шёлком, чтобы он усмирил своих евнухов.', '金と絹で懐柔し、党の統制を引き換えに。', 'Mit Gold und Seide bestechen, damit er seine Eunuchen zügelt.'),
 q('阉党敛迹，列国称便，惟库藏见薄。', '閹黨斂跡，列國稱便，惟庫藏見薄。', 'The eunuchs quiet down and foreign courts approve — but the vaults run thin.', 'Евнухи притихли, чужие дворы довольны — но казна редеет.', '閹党がおとなしくし、列国も安堵——ただ庫が細る。', 'Die Eunuchen werden still, und fremde Höfe zeigen sich zufrieden — doch die Kassen dünnen aus.'),
 q('雷霆清宫', '雷霆清宮', 'Purge the palace', 'Очистить дворец', '宮を清める', 'Den Palast säubern'),
 q('大索阉党，明正典刑，株连勿论。', '大索閹黨，明正典刑，株連勿論。', 'Sweep the eunuch faction and judge them by law, no matter the fallout.', 'Пройтись по фракции евнухов и судить по закону.', '閹党を大捜索し、法に照らして裁く——連坐も辞さず。', 'Die Eunuchenfraktion ausheben und nach Gesetz richten, die Folgen hin oder her.'),
 q('阉党伏诛，然牵连甚广，宫闱连日不安。', '閹黨伏誅，然牽連甚廣，宮闈連日不安。', 'The eunuchs fall, but the net is wide and the palace is uneasy for days.', 'Евнухи казнены, но сеть широка, и дворец ещё долго тревожен.', '閹党は誅されたが、連坐は広がり、宮闈は連日騒がしい。', 'Die Eunuchen fallen, aber das Netz ist weit; der Palast bleibt tagelang in Unruhe.')
)

# ===== 太后垂帘 → 摄政改制（跨年连锁）=====
ev('dowager_veil', 2,
 ('太后垂帘', '太后垂簾', 'The Dowager\'s Veil', 'Завеса вдовствующей государыни', '太后の垂簾', 'Der Schleier der Witwenkaiserin'),
 ('太后以{king}尚幼为由，垂帘临朝，帘后之声压过御座，朝议两分。', '太后以{king}尚幼為由，垂簾臨朝，簾後之聲壓過御座，朝議兩分。', 'The dowager claims {king} is young and rules from behind the veil; her voice outweighs the throne, and the court divides.', 'Вдовствующая государыня правит из-за завесы, ссылаясь на юность {king}; её голос весомее трона, и двор разделился.', '太后は{king}の幼少を理由に垂簾臨朝——簾の声が御座を圧し、朝議は二つに割れる。', 'Die Witwenkaiserin hält sich an {king}\'s Jugend und regiert hinter dem Vorhang; ihre Stimme überwiegt den Thron, und der Hof teilt sich.'),
 q('许其垂帘', '許其垂簾', 'Yield the veil', 'Уступить завесу', '垂簾を許す', 'Den Vorhang gewähren'),
 q('允太后临朝称制，权柄暂移帘后。', '允太后臨朝稱制，權柄暫移簾後。', 'Allow the dowager to sit in state; power moves behind the veil for now.', 'Допустить государыню к власти — пока за завесой.', '太后の臨朝称制を許し、権を暫く簾の後ろへ。', 'Der Witwenkaiserin den Thronsitz erlauben; die Macht wandert hinter den Vorhang.'),
 q('太后临朝，政令两出，帘后之影渐深……', '太后臨朝，政令兩出，簾後之影漸深……', 'The dowager holds court; edicts issue in two voices... the shadow behind the veil deepens.', 'Государыня правит; указы выходят двумя голосами... тень за завесой растёт.', '太后が朝を執り、政令は二つに出る——簾の影が次第に深くなる……', 'Die Witwenkaiserin hält Hof; Erlasse kommen aus zwei Stimmen... der Schatten hinter dem Vorhang wächst.'),
 q('恭请还政', '恭請還政', 'Plead for return', 'Просить вернуть власть', '還政を請う', 'Rückgabe erbitten'),
 q('率群臣叩请还政，礼数周全而不让步。', '率群臣叩請還政，禮數周全而不讓步。', 'Lead the courtiers in asking her to return the reins — courtly in form, firm in substance.', 'Возглавить просьбу вернуть бразды правления — вежливо, но твёрдо.', '群臣を率いて還政を要請——礼を尽くしつつ一歩も譲らない。', 'Mit den Höflingen um die Rückgabe bitten — höflich in der Form, fest in der Sache.'),
 q('太后暂收回帘，列国传{king}亲政之志。', '太后暫收回簾，列國傳{king}親政之志。', 'The dowager withdraws; foreign courts hear of {king}\'s will to rule in person.', 'Государыня отступает; чужие дворы слышат о решимости {king} править самому.', '太后が一旦引く——列国に{king}の親政の志が伝わる。', 'Die Witwenkaiserin zieht sich zurück; fremde Höfe vernehmen {king}\'s Willen, selbst zu regieren.')
)
ev('regent_reform', 2,
 ('摄政改制', '攝政改制', 'The Regent\'s Reform', 'Реформа регента', '摂政の改革', 'Die Reform der Regentin'),
 ('垂帘太后欲行新政、更定祖制，朝野沸然，{king}如何表态？', '垂簾太后欲行新政、更定祖制，朝野沸然，{king}如何表態？', 'The veiled dowager moves to reform the founding laws; the court seethes. How shall {king} speak?', 'Государыня за завесой задумала новые порядки и пересмотр устоев; двор бурлит. Как скажет {king}?', '垂簾の太后が新政と祖制の改定を進める——朝野が沸騰し、{king}は如何に応えるか。', 'Die verschleierte Witwenkaiserin will die Grundgesetze reformieren; der Hof brodelt. Wie äußert sich {king}?'),
 q('力主新政', '力主新政', 'Back the reform', 'Поддержать реформу', '新政を推す', 'Die Reform stützen'),
 q('首肯新政，革故鼎新，整饬法度。', '首肯新政，革故鼎新，整飭法度。', 'Approve the new course: cast out the old, renew the laws.', 'Одобрить новый курс: отвергнуть старое, обновить законы.', '新政を認め、古を捨て法度を整える。', 'Den neuen Kurs billigen: das Alte verwerfen, die Gesetze erneuern.'),
 q('新政推行，列国侧目，皆言{kingdom}朝气正盛。', '新政推行，列國側目，皆言{kingdom}朝氣正盛。', 'The reform takes hold; foreign courts take note and call {kingdom} full of vigor.', 'Реформа прижилась; чужие дворы называют {kingdom} полным сил.', '新政が行われると列国が注目し、{kingdom}の盛んな気勢を言う。', 'Die Reform greift; fremde Höfe nennen {kingdom} voller Tatkraft.'),
 q('祖制为先', '祖制為先', 'Keep the old ways', 'Держаться устоев', '祖制を守る', 'Beim Alten bleiben'),
 q('以祖制不可轻动，驳回新政之议。', '以祖制不可輕動，駁回新政之議。', 'The founding laws are not lightly shaken; reject the reform bill.', 'Устои нельзя колебать; отклонить реформу.', '祖制は安易に動かせぬとして新政の議を退ける。', 'Die Grundgesetze wankt man nicht; die Reform verwerfen.'),
 q('新政废止，老臣弹冠，朝野各怀其怨。', '新政廢止，老臣彈冠，朝野各懷其怨。', 'The reform dies; old ministers preen, but the court nurses mutual grudges.', 'Реформа умерла; старые сановники ликуют, но двор затаил взаимные обиды.', '新政は廃され、老臣は鼻高々——だが朝野に怨みが残る。', 'Die Reform stirbt; alte Minister triumphieren, doch der Hof hegt gegenseitigen Groll.')
)

# ===== 内阁与储君 =====
ev('crown_prince_visit', 2,
 ('太子巡国', '太子巡國', 'The Prince\'s Tour', 'Объезд принца', '太子の巡幸', 'Die Rundreise des Thronfolgers'),
 ('储君已冠，请代{king}巡视列国，众臣于此各执一词，难有定论。', '儲君已冠，請代{king}巡視列國，眾臣於此各執一詞，難有定論。', 'The crown prince, now of age, asks to tour the foreign courts for {king}; the ministers are split and no verdict comes.', 'Совершеннолетний наследник просит объехать чужие дворы от имени {king}; советники разошлись, и вывода нет.', '冠を済ませた太子が{king}に代わり列国を巡ることを願う——群臣の意見は割れ、定論がない。', 'Der volljährige Thronfolger bittet, für {king} die fremden Höfe zu bereisen; die Minister sind gespalten, und kein Urteil findet sich.'),
 q('遣其出巡', '遣其出巡', 'Send him abroad', 'Отправить в путь', '巡幸に遣わす', 'Ihn aussenden'),
 q('遣太子代巡列国，以储君之礼相迎。', '遣太子代巡列國，以儲君之禮相迎。', 'Send him forth so foreign courts may greet the heir as a prince.', 'Отправить наследника, чтобы чужие дворы встретили его как принца.', '太子を遣わせば、列国は儲君の礼で迎える。', 'Ihn aussenden, damit fremde Höfe den Erben als Fürsten empfangen.'),
 q('太子所至，列国礼遇备至，{kingdom}邦交大畅。', '太子所至，列國禮遇備至，{kingdom}邦交大暢。', 'Wherever the prince goes, honors abound; {kingdom}\'s foreign ties flourish.', 'Куда ни приедет принц — почести; связи {kingdom} зацветают.', '太子の赴く先々で列国が厚遇——{kingdom}の国交が大いに開ける。', 'Wo der Prinz hinkommt, warten Ehren; die Beziehungen von {kingdom} blühen.'),
 q('留宫就学', '留宮就學', 'Keep him home', 'Оставить дома', '宮に留める', 'Zu Hause behalten'),
 q('以学未成、事未识为由留宫读书。', '以學未成、事未識為由留宮讀書。', 'His studies are unfinished; keep him by the hearth and the books.', 'Учёба не завершена — оставить дома, к книгам.', '学が未熟と理由を付け、宮に残して書を読ませる。', 'Die Studien sind unfertig — ihn daheim bei den Büchern lassen.'),
 q('太子读书守宫，诸事如常，波澜不惊。', '太子讀書守宮，諸事如常，波瀾不驚。', 'The prince studies at court; all is as it was, no wave stirred.', 'Принц учится при дворе; всё как прежде, без волны.', '太子は宮で書を読み、物事は平常——波も立たず。', 'Der Prinz studiert am Hof; alles bleibt, wie es war, keine Welle rührt sich.')
)
ev('royal_confessor', 2,
 ('御前谏臣', '御前諫臣', 'The Royal Confessor', 'Королевский духовник', '御前の諫臣', 'Der Beichtvater des Königs'),
 ('御前谏臣叩请{king}恢复每日听政仪轨，谓此正纲纪、肃百僚，言辞切切。', '御前諫臣叩請{king}恢復每日聽政儀軌，謂此正綱紀、肅百僚，言辭切切。', 'The royal confessor begs {king} to restore the daily audience rite, saying it will right the order and quell the courtiers; his plea is earnest.', 'Королевский духовник просит {king} вернуть ежедневную аудиенцию — так восстановится порядок и притихнут сановники; речь его пылка.', '御前の諫臣が{king}に日々の聴政の儀軌を復すよう願う——綱紀を正し百官を整えると、その言葉は切々たるもの。', 'Der Beichtvater bittet {king}, die tägliche Audienz wieder einzuführen, da sie die Ordnung richte und die Beamten zügele; seine Bitte ist inständig.'),
 q('恢复仪轨', '恢復儀軌', 'Restore the rite', 'Вернуть церемонию', '儀軌を復す', 'Die Rite wiederherstellen'),
 q('依言恢复听政仪轨，端正纲纪。', '依言恢復聽政儀軌，端正綱紀。', 'Do as he asks: restore the rite and set the order straight.', 'Исполнить просьбу: вернуть церемонию и навести порядок.', '言う通りに儀軌を復し、綱紀を正す。', 'Wie erbatten: die Rite wiederherstellen und die Ordnung richten.'),
 q('朝纲肃然，列国使节皆赞{king}勤政。', '朝綱肅然，列國使節皆讚{king}勤政。', 'The court stands in order; envoys praise {king}\'s diligence.', 'Двор в порядке; послы хвалят усердие {king}.', '朝綱が整い、列国の使節も{king}の勤政を讚える。', 'Der Hof steht in Ordnung; Gesandte rühmen {king}\'s Eifer.'),
 q('另辟日程', '另闢日程', 'Set a lighter pace', 'Облегчить распорядок', '日程を改める', 'Sanfteren Rhythmus wählen'),
 q('改择吉日听政，不落繁文缛节。', '改擇吉日聽政，不落繁文縟節。', 'Pick an auspicious day instead, without the heavy formalities.', 'Назначить счастливый день, без тяжких церемоний.', '吉日を選んで聴政し、煩文縟礼に落ちない。', 'Einen günstigen Tag wählen, ohne schweres Zeremoniell.'),
 q('仪轨未复，谏臣喟叹而去，自此上疏渐稀。', '儀軌未復，諫臣喟嘆而去，自此上疏漸稀。', 'The rite stays unreformed; the confessor sighs and departs, and memorials grow rare from then on.', 'Церемония не возвращена; духовник вздыхает и уходит, и с тех пор прошений всё меньше.', '儀軌は復さず、諫臣は嘆きつつ去る——以後、上奏が次第に減る。', 'Die Rite bleibt unerneuert; der Beichtvater seufzt und geht, und die Eingaben werden seither selten.')
)
ev('palace_oath', 2,
 ('宫廷盟誓', '宮廷盟誓', 'The Palace Oath', 'Дворцовая клятва', '宮廷の盟誓', 'Der Palasteid'),
 ('近臣欲与{king}结誓共进退，誓毕请赐盟物，礼官以为非制。', '近臣欲與{king}結誓共進退，誓畢請賜盟物，禮官以為非制。', 'A close minister seeks an oath of shared fortune with {king}, asking for a token; the rite-master calls it unheard of.', 'Ближний сановник хочет клятвы единства с {king}, просит дар; церемониймейстер называет это беспрецедентным.', '近臣が{king}と共に进退の誓いを結び、盟物を賜りたいと願う——礼官は非制と見る。', 'Ein enger Minister sucht einen Eid gemeinsamer Sache mit {king} und bittet um ein Zeichen; der Zeremonienmeister nennt es beispiellos.'),
 q('赐物结誓', '賜物結誓', 'Seal the oath', 'Скрепить клятву', '物を賜い盟を結ぶ', 'Den Eid besiegeln'),
 q('赐以玉佩结誓，君臣自此一心。', '賜以玉佩結誓，君臣自此一心。', 'Give a jade token and seal the oath of one heart.', 'Даровать яшмовый знак и скрепить клятву единого сердца.', '玉佩を賜い、君臣一心の盟を結ぶ。', 'Ein Jadestück schenken und den Eid einen Herzens besiegeln.'),
 q('盟誓既成，近臣效力，列国闻之亦敬。', '盟誓既成，近臣效力，列國聞之亦敬。', 'The oath is sworn; the minister serves; even foreign courts hold it worthy.', 'Клятва дана; сановник служит; даже чужие дворы признают.', '盟が成り、近臣が力を尽くす——列国もこれを敬う。', 'Der Eid ist geschworen; der Minister dient; selbst fremde Höfe halten ihn für würdig.'),
 q('礼官驳回', '禮官駁回', 'Reject the rite', 'Отказать в обряде', '礼官が退ける', 'Den Ritus ablehnen'),
 q('以私盟乱君臣之礼，驳回此议。', '以私盟亂君臣之禮，駁回此議。', 'A private oath upsets the order of lord and minister; refuse it.', 'Частная клятва рушит порядок государя и сановника — отказать.', '私盟は君臣の礼を乱すとして退ける。', 'Ein Privateid stört die Ordnung von Herr und Diener; ablehnen.'),
 q('近臣羞忿，朝中私议纷纷，忠奸难辨。', '近臣羞忿，朝中私議紛紛，忠奸難辨。', 'The minister is stung with shame; whispers fill the court and loyalty clouds.', 'Сановник уязвлён; дворец полон шёпота, верность под облаками.', '近臣が恥と怒りに悶え、朝中の私議が渦巻き忠奸は見分け難し。', 'Der Minister ist beschämt; Flüstern erfüllt den Hof, und Loyalität verschwimmt.')
)
ev('hunt_omen', 2,
 ('猎场凶兆', '獵場凶兆', 'An Ill Omen at the Hunt', 'Дурное знамение на охоте', '狩場の凶兆', 'Ein böses Omen auf der Jagd'),
 ('秋狝途中，{king}一箭中鹿而鹿啼泣不止，血泪斑斑，随从皆言不妥。', '秋獮途中，{king}一箭中鹿而鹿啼泣不止，血淚斑斑，隨從皆言不妥。', 'On the autumn hunt {king}\'s arrow finds the deer, yet it weeps without end, tears of blood about; the retinue calls it an ill omen.', 'На осенней охоте стрела {king} поразила оленя, но тот рыдает кровавыми слезами; свита шепчет о дурном знамении.', '秋の狩りで{king}の矢が鹿を射抜く——しかし鹿は血の涙を流して泣き止まぬ。お供の者たちは皆、これを不吉の兆と囁く。', 'Auf der Herbstjagd trifft {king}\'s Pfeil den Hirsch, doch er weint ohne Ende, Tränen aus Blut; das Gefolge nennt es ein böses Omen.'),
 q('严究流言', '嚴究流言', 'Crush the talk', 'Пресечь толки', '流言を断つ', 'Die Reden ersticken'),
 q('严究散布凶兆者，以安众人之心。', '嚴究散佈凶兆者，以安眾人之心。', 'Hunt down those who spread the omen and steady the hearts of all.', 'Найти распространителей знамения и успокоить всех.', '凶兆を流す者の筋を厳しく追及し、人心を安んじる。', 'Die Verbreiter des Omens verfolgen und alle Herzen beruhigen.'),
 q('流言虽止，人心疑惧未消，猎场气氛诡谲。', '流言雖止，人心疑懼未消，獵場氣氛詭譎。', 'The talk dies, but dread lingers; the hunt ground grows strange.', 'Толки умолкли, но тревога осталась; охотничий стан странен.', '流言は止まるも疑惧は消えず、狩場の空気が怪しい。', 'Das Gerede stirbt, doch das Bangen bleibt; der Jagdplatz wird unheimlich.'),
 q('一笑置之', '一笑置之', 'Laugh it off', 'Отшутиться', '一笑に付す', 'Darüber lachen'),
 q('笑言天命在我，照常围猎。', '笑言天命在我，照常圍獵。', 'Laugh: heaven\'s mandate is mine — and continue the hunt.', 'Рассмеяться: небо за нас — и продолжать охоту.', '天命われにありと笑い、狩りを続ける。', 'Lachen: des Himmels Mandat ist mein — und weiterjagen.'),
 q('众人稍安，猎事照旧，兽获颇丰。', '眾人稍安，獵事照舊，獸獲頗豐。', 'All calm a little; the hunt goes on as ever, with a rich catch.', 'Все немного успокоились; охота идёт своим чередом, добыча богата.', '皆いくらか安堵し、狩りは例の通り——獲物は豊か。', 'Alle beruhigen sich etwas; die Jagd geht wie gewohnt weiter, mit reicher Beute.')
)
ev('heir_tutor', 2,
 ('储君择师', '儲君擇師', 'The Heir\'s Tutor', 'Наставник наследника', '太子の師', 'Der Lehrer des Thronfolgers'),
 ('储君年齿渐长，宫中延请名师之议再起，{king}欲择其善者。', '儲君年齒漸長，宮中延請名師之議再起，{king}欲擇其善者。', 'The heir grows in years; talk of a famed tutor revives, and {king} ponders the wisest choice.', 'Наследник подрастает; снова заговорили о знаменитом наставнике, и {king} размышляет о выборе.', '太子の年が進み、名師を招く議が再び持ち上がる——{king}は良き師を選ばんとする。', 'Der Erbe wird älter; die Rede von einem berühmten Lehrer lebt auf, und {king} wägt die beste Wahl.'),
 q('延聘名儒', '延聘名儒', 'Hire the sage', 'Пригласить мудреца', '名儒を招く', 'Den Weisen anwerben'),
 q('聘天下名儒为傅，教以诗书礼法。', '聘天下名儒為傅，教以詩書禮法。', 'Hire a famous scholar as tutor of poetry, books, rites and law.', 'Нанять известного учёного — учить поэзии, книгам, обряду и закону.', '天下の名儒を傅に聘し、詩書礼法を教えしむ。', 'Einen berühmten Gelehrten als Lehrer für Dichtung, Bücher, Riten und Recht anwerben.'),
 q('名师入宫，储君进益，列国交口称善。', '名師入宮，儲君進益，列國交口稱善。', 'The sage enters court; the heir improves; foreign courts praise it with one voice.', 'Мудрец при дворе; наследник растёт; чужие дворы хвалят в один голос.', '名師が宮に入り太子が進歩——列国が口々に称賛する。', 'Der Weise kommt an den Hof; der Erbe gedeiht; fremde Höfe loben es einhellig.'),
 q('宗学自教', '宗學自教', 'Royal tutors suffice', 'Обойтись своими', '宗学で教う', 'Hauslehrer genügen'),
 q('宗学自有教习，不必外求。', '宗學自有教習，不必外求。', 'The royal school has masters; no outside hand is needed.', 'У дворца есть свои учителя; чужие не нужны.', '宗学に教習あり、外に求めるに及ばず。', 'Die Hofschule hat Meister; fremde Hände sind unnötig.'),
 q('储君依旧就学宗学，谨守成规，波澜不惊。', '儲君依舊就學宗學，謹守成規，波瀾不驚。', 'The heir keeps to the royal school and its old rules; no wave is stirred.', 'Наследник остаётся при своей школе и её уставах; волны нет.', '太子は引き続き宗学に通い、成規を守る——波は立たず。', 'Der Erbe besucht weiter die Hofschule und ihre alten Regeln; keine Welle rührt sich.')
)
ev('concubine_audit', 2,
 ('后宫清查', '後宮清查', 'The Harem Audit', 'Ревизия гарема', '後宮の監査', 'Die Harem-Prüfung'),
 ('密报称宫中藏有违禁之物，礼官请{king}清查六宫，妃嫔人人自危。', '密報稱宮中藏有違禁之物，禮官請{king}清查六宮，妃嬪人人自危。', 'A tip says forbidden goods hide in the palace; the rite-master asks {king} to search every wing, and the ladies tremble.', 'Донос: во дворце спрятан запретный товар; церемониймейстер просит {king} обыскать дворец, дамы дрожат.', '密報に宮中に禁物が隠されていると——礼官が{king}に六宮の捜査を願い、妃嬪たちは皆怯える。', 'Eine Anzeige: Verbotenes sei im Palast versteckt; der Zeremonienmeister bittet {king}, jeden Flügel zu durchsuchen, die Damen beben.'),
 q('彻查六宫', '徹查六宮', 'Search every wing', 'Обыскать всё', '六宮を捜査', 'Jeden Flügel durchsuchen'),
 q('不徇私情，逐宫搜查，依律严办。', '不徇私情，逐宮搜查，依律嚴辦。', 'Show no favor; search room by room and judge by law.', 'Без поблажек: обыскать покои и судить по закону.', '私情を許さず、宮ごとに捜して法に依り厳しく処す。', 'Keine Gnade: Raum für Raum durchsuchen und nach Gesetz richten.'),
 q('查出私弊数桩，妃嫔受惊，宫中怨声渐起。', '查出私弊數樁，妃嬪受驚，宮中怨聲漸起。', 'Several abuses surface; the ladies are shaken and grumbling spreads through the court.', 'Несколько злоупотреблений вскрыто; дамы потрясены, во дворце ропот.', '私弊が数件暴かれ、妃嬪らが驚悸——宮中に怨嗟が燻る。', 'Mehrere Übeltaten kommen ans Licht; die Damen sind erschüttert, Groll zieht durch den Palast.'),
 q('从宽查问', '從寬查問', 'A lenient look', 'Мягко проверить', '寛く問う', 'Milde prüfen'),
 q('召主事者问话，从宽发落。', '召主事者問話，從寬發落。', 'Call the chief attendants and deal gently with them.', 'Позвать старших слуг и обойтись мягко.', '主任の者を召して問い、寛に扱う。', 'Die Obersten rufen und milde richten.'),
 q('查无大弊，后宫得以安宁，流言自息。', '查無大弊，後宮得以安寧，流言自息。', 'No great offense is found; the harem keeps its peace and the rumor fades.', 'Большого вреда не нашли; гарем сохраняет покой, молва тает.', '大きな弊なし——後宮は安寧を得、流言も自ら消える。', 'Kein großes Vergehen; der Harem bewahrt seinen Frieden, und das Gerücht verweht.')
)
ev('marriage_plot', 2,
 ('联姻图谋', '聯姻圖謀', 'A Marital Scheme', 'Брачный замысел', '縁組みの謀', 'Ein Heiratsplan'),
 ('邻国遣使求娶王妹，其国相却密见{king}，许奸细以重贿，图里应外合。', '鄰國遣使求娶王妹，其國相卻密見{king}，許奸細以重賄，圖裡應外合。', 'A neighbor seeks the princess, yet its minister secretly meets {king} and offers bribes on behalf of spies, plotting from within and without.', 'Сосед просит руки принцессы, но его министр тайно видит {king} и предлагает взятки за лазутчиков, готовя удар изнутри и снаружи.', '隣国が王妹を娶りたく遣使——しかしその国相は{king}に密会し、間者への重賄を申し出て、内外から呼応せんと謀る。', 'Ein Nachbar wirbt um die Prinzessin, doch sein Minister sucht heimlich {king} und bietet Bestechung für Spione, um von innen und außen zu schlagen.'),
 q('许其联姻', '許其聯姻', 'Grant the match', 'Согласиться на брак', '縁組を許す', 'Die Ehe gestatten'),
 q('许以王妹下嫁，两国结为姻亲。', '許以王妹下嫁，兩國結為姻親。', 'Give the princess in marriage; the two dynasties become kin.', 'Отдать принцессу; две династии становятся роднёй.', '王妹を嫁がせ、両国が姻戚となる。', 'Die Prinzessin vermählen; zwei Dynastien werden verwandt.'),
 q('婚约既成，列国来贺，{kingdom}邦交纳长。', '婚約既成，列國來賀，{kingdom}邦交納長。', 'The contract sets; foreign courts come to congratulate; {kingdom}\'s ties grow long.', 'Договор заключён; чужие дворы поздравляют; связи {kingdom} растут.', '婚約が成り、列国が祝いに来る——{kingdom}の国交が伸びる。', 'Der Vertrag steht; fremde Höfe gratulieren; die Bande von {kingdom} wachsen.'),
 q('察其异志', '察其異志', 'Smell the plot', 'Разглядеть уловку', '異心を察す', 'Die List wittern'),
 q('密查其国相贿赂之事，婉拒求亲。', '密查其國相賄賂之事，婉拒求親。', 'Privately probe the bribery and decline the suit with courtesy.', 'Справки о взятках — и вежливо отказать.', '国相の賄賂を密かに調べ、縁談を婉曲に断る。', 'Der Bestechung privat nachgehen und die Werbung höflich ablehnen.'),
 q('拒婚之事传开，朝中议论纷纷，君臣互相猜忌。', '拒婚之事傳開，朝中議論紛紛，君臣互相猜忌。', 'The refusal spreads; the court buzzes and lord and minister eye each other.', 'Отказ разнёсся; двор гудит, государь и сановники смотрят друг на друга.', '拒婚が広まり、朝中の議論が渦巻き、君臣が互いを猜む。', 'Die Ablehnung spricht sich herum; der Hof raunt, Herr und Diener beäugen einander.')
)
ev('royal_bodyguard', 2,
 ('御前侍卫', '御前侍衛', 'The Royal Bodyguard', 'Королевская стража', '御前の衛士', 'Die königliche Leibwache'),
 ('近臣进言：御前侍卫久疏整饬，老弱渐多，请{king}斥资整训换防。', '近臣進言：御前侍衛久疏整飭，老弱漸多，請{king}斥資整訓換防。', 'A close minister says the bodyguard has slackened and grown old and weak; he begs {king} to fund drill and rotation.', 'Ближний сановник: стража распустилась и постарела — просит {king} дать деньги на муштру и смену.', '近臣が言う——御前の衛士は長く手入れなされず、老いた者も多い。{king}に費を投じ訓練と交代を願う。', 'Ein Minister meldet, die Leibwache habe nachgelassen und sei alt geworden; er bittet {king}, Übung und Wechsel zu finanzieren.'),
 q('斥资整训', '斥資整訓', 'Fund the drill', 'Деньги на муштру', '費を投じ訓練', 'Die Übung finanzieren'),
 q('拨金整训侍卫，汰换老朽。', '撥金整訓侍衛，汰換老朽。', 'Vault gold into drill and retire the worn-out men.', 'Вложиться в муштру и распустить одряхлевших.', '金を投じ衛士を訓練し、老いた者を入れ替える。', 'Gold in die Übung stecken und die Ausgedienten ersetzen.'),
 q('侍卫焕然一新，御前再无人敢懈怠。', '侍衛煥然一新，御前再無人敢懈怠。', 'The guard stands renewed; no one dares slacken by the throne.', 'Стража обновлена; никто не смеет лениться у трона.', '衛士が一新され、御前で怠ける者はいない。', 'Die Wache ist erneuert; niemand wagt mehr, am Thron zu trödeln.'),
 q('暂缓更换', '暫緩更換', 'Hold off', 'Повременить', '見合わせる', 'Aufschieben'),
 q('库帑吃紧，整训之事暂缓。', '庫帑吃緊，整訓之事暫緩。', 'The vaults are tight; set the drill aside.', 'Казна тесна; муштру отложить.', '庫が逼迫、訓練は見合わせる。', 'Die Kassen sind knapp; die Übung aufschieben.'),
 q('卫队依旧，御前如常，只是戒备渐弛。', '衛隊依舊，御前如常，只是戒備漸弛。', 'The guard stays as it was; the throne is as ever, only the watchfulness slackens.', 'Стража прежняя; у трона всё как прежде, лишь бдительность слабеет.', '衛士は従来通り、御前も平常——ただ警戒が次第に緩む。', 'Die Wache bleibt, wie sie ist; der Thron wie immer — nur die Wachsamkeit lässt nach.')
)

# ===== 言官与库藏 =====
ev('eunuch_land', 2,
 ('宦官隐田', '宦官隱田', 'The Eunuchs\' Lands', 'Земли евнухов', '宦官の隠田', 'Die Ländereien der Eunuchen'),
 ('御史纠劾宦官占据隐田数百顷，赋税不入国库，请{king}裁断。', '御史糾劾宦官佔據隱田數百頃，賦稅不入國庫，請{king}裁斷。', 'The censor impeaches eunuchs for holding hundreds of hidden acres that pay no tax; he begs {king} to judge.', 'Цензор обличает евнухов: сотни скрытых акров не платят налога — просит {king} рассудить.', '御史が宦官を弾劾——隠田数百頃が税を納めぬと。{king}に裁きを請う。', 'Der Zensor klagt Eunuchen an: Hunderte versteckte Äcker zahlen keine Steuer — er bittet {king} zu richten.'),
 q('清查充公', '清查充公', 'Confiscate the land', 'Конфисковать', '没収する', 'Land einziehen'),
 q('派员丈量隐田，尽数没入官仓。', '派員丈量隱田，盡數沒入官倉。', 'Send surveyors, measure the hidden acres, and seize them for the crown.', 'Выслать землемеров и отобрать скрытые угодья в казну.', '役人を遣わし隠田を測量、すべて官に没収する。', 'Vermesser senden, die versteckten Äcker vermessen und für die Krone einziehen.'),
 q('隐田充公，岁入大增，国库为之充盈。', '隱田充公，歲入大增，國庫為之充盈。', 'The lands are seized, the revenue swells, and the vaults fill.', 'Угодья отобраны; доходы выросли; казна полнится.', '隠田が没収され、歳入が大いに増え庫が満ちる。', 'Die Äcker sind eingezogen; die Einkünfte wachsen; die Kassen füllen sich.'),
 q('按下不表', '按下不表', 'Sidestep it', 'Отложить дело', 'うやむやにする', 'Die Sache vertagen'),
 q('投鼠忌器，暂且按下不表。', '投鼠忌器，暫且按下不表。', 'To strike could harm the cup; set the matter silently aside.', 'Удар может задеть сосуд — тихо отложить.', '器が恐ろしく、しばらく沙汰止みにする。', 'Der Schlag könnte das Gefäß berühren — die Sache schweigend beiseitelegen.'),
 q('阉党益骄，朝臣巡捕皆敢怒不敢言。', '閹黨益驕，朝臣巡捕皆敢怒不敢言。', 'The eunuchs grow prouder; officials seethe but dare not speak.', 'Евнухи становятся наглее; сановники кипят, но молчат.', '閹党はいよいよ傲慢に——臣も巡捕も怒りつつ口を閉ざす。', 'Die Eunuchen werden überheblicher; Beamte kochen, wagen aber kein Wort.')
)
ev('censor_impeach', 2,
 ('御史弹劾', '御史彈劾', 'The Censor\'s Impeachment', 'Импечмент цензора', '御史の弾劾', 'Die Anklage des Zensors'),
 ('御史弹劾近臣贪墨，账目铁证俱在，{king}的朝堂一时鸦雀无声。', '御史彈劾近臣貪墨，賬目鐵證俱在，{king}的朝堂一時鴉雀無聲。', 'The censor impeaches a favorite for graft; the ledgers are proof, and {king}\'s court falls dead silent.', 'Цензор обвиняет фаворита в казнокрадстве; счета — неопровержимое доказательство; двор {king} замер.', '御史が近臣の汚職を弾劾——帳簿という動かぬ証拠が揃い、{king}の朝堂がしんと静まる。', 'Der Zensor klagt einen Günstling der Unterschlagung an; die Bücher sind Beweis, und {king}\'s Hof verstummt.'),
 q('廷审穷治', '廷審窮治', 'Try it at court', 'Судить при дворе', '廷で究明', 'Vom Hof verhandeln'),
 q('命三司会审，务须穷治到底。', '命三司會審，務須窮治到底。', 'Order the tribunals to sit and dig to the bottom.', 'Велеть судам засесть и докопаться до дна.', '三司会審を命じ、底まで究める。', 'Die Tribunale tagen lassen und bis auf den Grund graben.'),
 q('涉案近臣下狱，牵连者众，朝局震荡不已。', '涉案近臣下獄，牽連者眾，朝局震盪不已。', 'The favorite falls; the net catches many, and the court rocks without end.', 'Фаворит пал; сеть захватила многих; двор качается.', '近臣が下獄し、連座多し——朝局が揺れ続ける。', 'Der Günstling fällt; das Netz erfasst viele, und der Hof schwankt unaufhörlich.'),
 q('留中不发', '留中不發', 'Stifle the knife', 'Замять', '握りつぶす', 'Ersticken'),
 q('将弹章留中，私下查问清楚。', '將彈章留中，私下查問清楚。', 'Keep the memorial unopened and inquire quietly.', 'Оставить доклад у себя и тихо разузнать.', '弾章を留中し、内々に問いただす。', 'Die Anklageschrift liegen lassen und still nachfragen.'),
 q('弹章留中，御史悻悻，事态渐息。', '彈章留中，御史悻悻，事態漸息。', 'The memorial stalls; the censor leaves vexed; the matter fades.', 'Доклад застрял; цензор уходит раздосадованным; дело тает.', '弾章は留中、御史は不満げに去り、事態は次第に収まる。', 'Die Schrift stockt; der Zensor zieht verärgert ab; die Sache verweht.')
)
ev('court_banquet', 2,
 ('宫廷盛宴', '宮廷盛宴', 'The Grand Banquet', 'Великий пир', '宮廷の盛宴', 'Das große Festmahl'),
 ('{king}寿辰将至，礼官请开琼筵款待列国使节，度支之臣力陈其奢。', '{king}壽辰將至，禮官請開瓊筵款待列國使節，度支之臣力陳其奢。', '{king}\'s birthday nears; the rite-master asks a great feast for envoys, but the budget keeper pleads extravagance.', 'День рождения {king} близко; церемониймейстер просит большой пир для послов, а казначей ворчит о роскоши.', '{king}の誕辰が近づく——礼官が列国使節を招く瓊筵を願うが、度支の臣がその奢を諫める。', '{king}\'s Geburtstag naht; der Zeremonienmeister bittet um ein Fest für die Gesandten, doch der Schatzmeister warnt vor Verschwendung.'),
 q('大开琼筵', '大開瓊筵', 'Feast the envoys', 'Угостить послов', '盛宴を開く', 'Die Gesandten bewirten'),
 q('张灯结彩，大宴列国使节。', '張燈結綵，大宴列國使節。', 'Hang the lamps, raise the banners and feast every envoy.', 'Повесить фонари и пировать с послами.', '灯を結び綵を張り、列国使節を大宴に招く。', 'Lichter aufziehen, Banner hissen und alle Gesandten bewirten.'),
 q('宴罢宾主尽欢，列国称{kingdom}好客有礼，惟金库见薄。', '宴罷賓主盡歡，列國稱{kingdom}好客有禮，惟金庫見薄。', 'The feast ends in joy; foreign courts praise {kingdom}\'s gracious hospitality — only the vault grows thin.', 'Пир окончен в радости; чужие дворы хвалят гостеприимство {kingdom} — лишь казна похудела.', '宴は大いに尽くされ、列国が{kingdom}の好客有礼を称ぶ——ただ庫が痩せる。', 'Das Fest endet in Freude; fremde Höfe rühmen die Gastlichkeit von {kingdom} — nur die Kassen dünnen aus.'),
 q('从俭致庆', '從儉致慶', 'Celebrate in thrift', 'Отметить скромно', '倹約に祝う', 'Sparsam feiern'),
 q('内廷小酌，使节但遣人致贺。', '內廷小酌，使節但遣人致賀。', 'A modest cup within the palace; envoys send their greetings only.', 'Скромная чаша во дворце; послы шлют только поздравления.', '内廷で小さな杯を挙げ、使節には祝いを遣わさせるのみ。', 'Ein bescheidener Becher im Palast; die Gesandten senden nur Glückwünsche.'),
 q('寿宴从俭，宾客尽散，波澜不兴。', '壽宴從儉，賓客盡散，波瀾不興。', 'The birthday passes in thrift; the guests disperse; no wave stirs.', 'День рождения прошёл скромно; гости разошлись; волны нет.', '寿宴は倹約に行われ、客は去り、波は立たず。', 'Der Geburtstag vergeht sparsam; die Gäste gehen; keine Welle rührt sich.')
)
ev('astrology_omen', 2,
 ('星曜示吉', '星曜示吉', 'An Auspicious Star', 'Счастливая звезда', '吉星の示す', 'Ein günstiger Stern'),
 ('钦天监夜观天象，见吉星入于{kingdom}分野，请颁诏宣示天下。', '欽天監夜觀天象，見吉星入於{kingdom}分野，請頒詔宣示天下。', 'The astrologers see a lucky star enter {kingdom}\'s quarter of the sky and ask to proclaim it to the world.', 'Звездочёты видят счастливую звезду в небе {kingdom} и просят возвестить об этом.', '欽天監が夜空を観て、{kingdom}の分野に吉星が入るのを見る——天下へ宣示するよう請う。', 'Die Sternkundigen sehen einen Glücksstern im Himmelsbezirk von {kingdom} und bitten um die Verkündung.'),
 q('颁诏宣示', '頒詔宣示', 'Proclaim it', 'Огласить', '詔で示す', 'Verkünden'),
 q('颁诏四方，谓天命在国，以昭祥瑞。', '頒詔四方，謂天命在國，以昭祥瑞。', 'Send forth the edict: heaven\'s favor rests upon the realm, a bright portent.', 'Огласить указ: небо благоволит державе — светлое знамение.', '四方に詔を頒ち、天命国に在りと告げ、瑞祥を照らす。', 'Den Erlass verkünden: des Himmels Gunst ruht auf dem Reich — ein helles Zeichen.'),
 q('瑞应传开，列国遣使来贺{kingdom}，使团络绎于途。', '瑞應傳開，列國遣使來賀{kingdom}，使團絡繹於途。', 'The auspice spreads; foreign courts send envoys to congratulate {kingdom}, missions strung along the roads.', 'Знак разнёсся; чужие дворы шлют послов поздравить {kingdom}, посольства вьются по дорогам.', '瑞応が広がり、列国から{kingdom}へ祝いの使者が来る——使節の列は道に続く。', 'Das Zeichen verbreitet sich; fremde Höfe senden Boten, {kingdom} zu gratulieren, und Gesandtschaften reihen sich auf den Straßen.'),
 q('秘而不宣', '秘而不宣', 'Keep it secret', 'Утаить', '内に秘す', 'Geheim halten'),
 q('谓天机不可轻泄，命其缄口。', '謂天機不可輕洩，命其緘口。', 'Heaven\'s word is not lightly told; command silence.', 'Слово неба не болтают — велеть молчать.', '天機は軽く漏らせぬとして、口を閉じさせよ。', 'Des Himmels Wort ist nicht leicht zu sagen; Schweigen befehlen.'),
 q('星语秘而不宣，宫中如常，然监官失望。', '星語秘而不宣，宮中如常，然監官失望。', 'The star is kept from tongues; the palace proceeds as always, though the starwatchers look disappointed.', 'Звезда остаётся тайной; дворец живёт как прежде, лишь звездочёты разочарованы.', '星の言葉は秘され、宮中は平常のまま——ただ観官の失望が残る。', 'Der Stern bleibt verschwiegen; der Palast geht wie immer weiter, doch die Sterngucker wirken enttäuscht.')
)
ev('royal_seal', 2,
 ('御玺风波', '御璽風波', 'The Lost Royal Seal', 'Пропажа печати', '御璽の波風', 'Das verschwundene Siegel'),
 ('内宫密报：御玺一夜失踪，疑为近侍所盗，{king}震怒，遂传全宫。', '內宮密報：御璽一夜失蹤，疑為近侍所盜，{king}震怒，遂傳全宮。', 'A secret word from the inner court: the royal seal vanished overnight, likely stolen by a servant; {king} burns, and the whole palace hears.', 'Тайный донос из дворца: печать исчезла за ночь, вероятно, её украли, — {king} в ярости, и весь дворец в тревоге.', '内宮の密報——御璽が一夜にして消えた。近侍の盗みと疑われ、{king}が激怒し、宮中を挙げて騒ぐ。', 'Heimliche Kunde aus dem Innenhof: das Königssiegel verschwand über Nacht, wohl von einem Diener gestohlen; {king} kocht, und der ganze Palast hört.'),
 q('大索宫闱', '大索宮闈', 'Search the palace', 'Обыскать дворец', '宮中を捜索', 'Den Palast durchsuchen'),
 q('封锁宫门，逐人搜检，绝不姑息。', '封鎖宮門，逐人搜檢，絕不姑息。', 'Bar the gates and search every person; show no mercy.', 'Запереть ворота и обыскать каждого без пощады.', '宮門を封鎖し、一人ずつ捜し調べる——容赦せず。', 'Die Tore sperren und jeden durchsuchen; keine Gnade.'),
 q('搜检多日未获，宫女内侍人人自危。', '搜檢多日未獲，宮女內侍人人自危。', 'Days of searching yield nothing; every maid and eunuch fears for himself.', 'Дни обысков безрезультатны; каждая служанка и евнух трепещут.', '何日も捜して見つからず——宮女も内侍も皆自らを危ぶむ。', 'Tage des Suchens ohne Fund; jede Magd und jeder Eunuch bangt um sich.'),
 q('重铸御玺', '重鑄御璽', 'Recast the seal', 'Перелить печать', '御璽を鋳直す', 'Das Siegel neu gießen'),
 q('造新玺备用，另密访旧玺下落。', '造新璽備用，另密訪舊璽下落。', 'Strike a new seal for use and quietly seek the old.', 'Отлить новую печать и тихо искать старую.', '新たな御璽を鋳て備え、旧璽の行方を密かに探る。', 'Ein neues Siegel prägen und still nach dem alten suchen.'),
 q('新玺既成，旧玺杳然，库藏又费一番。', '新璽既成，舊璽杳然，庫藏又費一番。', 'The new seal is done; the old stays lost, and the vaults have paid again.', 'Новая печать готова; старая пропала без вести; казна снова заплатила.', '新たな御璽は整い、旧璽は杳として知れず——庫がまた減る。', 'Das neue Siegel steht; das alte bleibt verschollen — die Kassen zahlten erneut.')
)
ev('succession_will', 2,
 ('先王遗诏', '先王遺詔', 'The Late King\'s Will', 'Завещание прежнего короля', '先王の遺詔', 'Das Testament des alten Königs'),
 ('宫中竹匣开启，得先王遗诏一纸，其言与现行继承之议相左。', '宮中竹匣開啟，得先王遺詔一紙，其言與現行繼承之議相左。', 'An old bamboo case opens to reveal the late king\'s will; its words run against the current succession talk.', 'Открыт бамбуковый ларец: завещание прежнего короля — его слова против нынешних толков о наследовании.', '宮中の竹匣が開かれ、先王の遺詔が現れる——その文は目下の継承の議と食い違う。', 'Eine Bambuslade öffnet sich: das Testament des alten Königs; seine Worte laufen den Gesprächen über die Nachfolge zuwider.'),
 q('奉诏而行', '奉詔而行', 'Follow the will', 'Следовать завещанию', '詔に従う', 'Dem Testament folgen'),
 q('依遗诏定嗣，早息朝野群议。', '依遺詔定嗣，早息朝野群議。', 'Settle the succession by the will and silence the talk of court and country.', 'Решить наследование по завещанию и утишить толки двора и державы.', '遺詔に依り後継を定め、朝野の群議を早く収める。', 'Die Nachfolge nach dem Testament regeln und das Gerede von Hof und Land stillen.'),
 q('遗诏既宣，名分遂定，列国皆言{king}守先人遗志。', '遺詔既宣，名分遂定，列國皆言{king}守先人遺志。', 'The will is read, the name fixed; foreign courts say {king} honors the forebear\'s wish.', 'Завещание оглашено, имена утверждены; чужие дворы говорят, что {king} чтит волю предка.', '遺詔が宣べられ名分が定まる——列国は{king}が先人の志を守ると言う。', 'Das Testament ist verlesen, der Name gesetzt; fremde Höfe sagen, {king} ehre den Willen des Ahnen.'),
 q('存疑待考', '存疑待考', 'Cast doubt', 'Подвергнуть сомнению', '疑いを留める', 'Zweifel anmelden'),
 q('指其笔迹存疑，另择他日再议。', '指其筆跡存疑，另擇他日再議。', 'Question the handwriting and revisit it another day.', 'Усомниться в почерке и отложить.', '筆跡に疑いを指し、他日に再議する。', 'Die Handschrift bezweifeln und auf einen anderen Tag vertagen.'),
 q('遗诏之真伪顿成党争，朝中对立如泾渭。', '遺詔之真偽頓成黨爭，朝中對立如涇渭。', 'The will\'s truth becomes a faction war; the court splits like river and water.', 'Истинность завещания стала фрондой; двор расколот, как река и вода.', '遺詔の真偽がたちまち党争に——朝中は水と油の如く対立する。', 'Des Testaments Wahrheit wird Parteienzwist; der Hof teilt sich wie Fluss und Wasser.')
)
ev('old_guard_pension', 2,
 ('老臣致仕', '老臣致仕', 'Old Ministers Retire', 'Отставка старых сановников', '老臣の致仕', 'Der Ruhestand der Alten'),
 ('随{king}征战多年的老臣年迈请辞，乞一份体面年金安度晚年。', '隨{king}征戰多年的老臣年邁請辭，乞一份體面年金安度晚年。', 'Ministers who fought beside {king} for decades, now aged, ask to retire with a decent pension.', 'Сановники, десятилетиями воевавшие рядом с {king}, просят отставки с достойной пенсией.', '{king}と戦い続けてきた老臣らが歳を重ねて辞を願う——体面ある年金を乞う。', 'Minister, die Jahrzehnte an {king}\'s Seite kämpften, bitten um Ruhestand mit anständiger Pension.'),
 q('厚赆遣归', '厚贐遣歸', 'Send them off richly', 'Щедро проводить', '厚く見送る', 'Freigebig verabschieden'),
 q('赐金帛田宅，风光致仕，全其体面。', '賜金帛田宅，風光致仕，全其體面。', 'Grant gold, silk, land and house — a glorious farewell that keeps their dignity.', 'Дать золото, шёлк, землю и дом — славные проводы, сохраняющие достоинство.', '金帛と田宅を賜い、壮麗に致仕させる——体面を全うして。', 'Gold, Seide, Land und Haus gewähren — ein ruhmvoller Abschied, der die Würde wahrt.'),
 q('老臣含泪拜别，库藏为之减色，然义名远扬。', '老臣含淚拜別，庫藏為之減色，然義名遠揚。', 'The old men take their leave in tears; the vaults have dimmed, yet their honor is carried far.', 'Старики прощаются со слезами; казна померкла, но о них говорят по всей округе.', '老臣が涙ながらに別れを告げ、庫はやや痩せる——しかし義名が遠くまで響く。', 'Die Alten nehmen tränenreich Abschied; die Kassen schimmern matter, doch ihr Ruhm trägt weit.'),
 q('减俸留任', '減俸留任', 'Keep them cheap', 'Оставить подешевле', '減俸で留任', 'Billig behalten'),
 q('留任闲职，俸禄减半，以省国用。', '留任閒職，俸祿減半，以省國用。', 'Keep them in sinecures at half pay to spare the treasury.', 'Оставить на синекурах с половинным жалованьем ради казны.', '閑職に留め、俸禄を半分にして国費を省く。', 'Sie in Sinekuren behalten, bei halbem Gehalt, um die Kasse zu schonen.'),
 q('老臣怨怼，交相叹{king}薄待旧人，朝中侧目。', '老臣怨懟，交相嘆{king}薄待舊人，朝中側目。', 'The old men seethe, sighing in chorus that {king} is cold to his own; the court glances sidelong.', 'Старики кипят, вздыхая хором, что {king} холоден к своим; двор косится.', '老臣らが怨み、口々に{king}の旧人を薄くすることを嘆く——朝中の目も冷たい。', 'Die Alten grollen und seufzen im Chor, {king} sei kalt zu den Seinen; der Hof schielt.')
)
ev('prince_exile', 2,
 ('流放亲王', '流放親王', 'A Prince in Exile', 'Изгнание принца', '親王の流罪', 'Ein Prinz im Exil'),
 ('亲王私通边将一事发于朝野，{king}震怒，朝议如何处置这位手足。', '親王私通邊將一事發於朝野，{king}震怒，朝議如何處置這位手足。', 'The prince\'s secret letters to a border general break open before court and country; {king} burns — how should this brother be judged?', 'Тайная переписка принца с пограничным генералом вскрылась при дворе; {king} в ярости — как судить брата?', '親王と辺将の内通が朝野に発覚——{king}激怒。この身内を如何に処すか、朝議が割れる。', 'Des Prinzen Briefe an einen Grenzgeneral brechen vor Hof und Land auf; {king} brennt — wie diesen Bruder richten?'),
 q('削爵流放', '削爵流放', 'Strip and exile', 'Лишить титула', '爵を削り流罪', 'Titel aberkennen, verbannen'),
 q('削其爵位，流放南荒，不得归京。', '削其爵位，流放南荒，不得歸京。', 'Strip his rank, exile him to the south wastelands, and bar his return.', 'Лишить титула, сослать в южную глушь и запретить возвращение.', '爵位を削り、南の荒野へ流す——帰京を許さず。', 'Rang aberkennen, in die südliche Wildnis verbannen und ihm die Rückkehr verwehren.'),
 q('亲王南行，列国窃议{king}手足相残，亲缘不复。', '親王南行，列國竊議{king}手足相殘，親緣不復。', 'The prince heads south; foreign courts whisper of {king} cutting his own flesh and blood.', 'Принц уезжает на юг; чужие дворы шепчут, что {king} рубит свою плоть и кровь.', '親王は南へ——列国は{king}の手足相食むを囁く。', 'Der Prinz zieht gen Süden; fremde Höfe raunen, {king} schneide ins eigene Fleisch.'),
 q('幽居封地', '幽居封地', 'House-arrest him', 'Домашний арест', '封地に幽居', 'Ihn auf seinem Land sperren'),
 q('命其闭门思过，节制用度。', '命其閉門思過，節制用度。', 'Order him behind locked gates to reflect, in reduced circumstance.', 'Велеть сидеть дома и размышлять, в урезанных условиях.', '門を閉じて思過し、用度を減らすよう命じる。', 'Ihn hinter verschlossenen Toren sinnen lassen, bei beschnittenem Aufwand.'),
 q('亲王府日夜有人把守，宗室人人自危。', '親王府日夜有人把守，宗室人人自危。', 'The prince\'s house is watched day and night; every royal kin dreads his own turn.', 'Дом принца под надзором; каждая родня дрожит за себя.', '親王邸は昼夜見張られ、宗室の誰もが自らを案ずる。', 'Des Prinzen Haus wird Tag und Nacht bewacht; jeder Verwandte bangt um sich.')
)
ev('royal_birth', 2,
 ('王子诞生', '王子誕生', 'A Prince Is Born', 'Рождение принца', '王子の誕生', 'Ein Prinz ist geboren'),
 ('王后诞下王子，啼声清亮，{king}喜不自胜，群臣称贺，卫队山呼。', '王后誕下王子，啼聲清亮，{king}喜不自勝，群臣稱賀，衛隊山呼。', 'The queen bears a prince — a clear, bright cry; {king} is overjoyed, courtiers congratulate, and the guards cheer them on.', 'Королева родила принца — ясный крик; {king} вне себя от радости, двор поздравляет, стража ликует.', '王妃が王子を産み落とす——啼声は清らか。{king}が歓喜に溢れ、群臣が祝い、衛士が万歳を叫ぶ。', 'Die Königin bringt einen Prinzen — ein heller Schrei; {king} ist überglücklich, die Höflinge gratulieren, und die Wache jubelt.'),
 q('大赦颁恩', '大赦頒恩', 'Amnesty and grace', 'Амнистия и милость', '大赦と恩賞', 'Amnestie und Gnade'),
 q('大赦天下，开仓与民同贺。', '大赦天下，開倉與民同賀。', 'Proclaim amnesty, open the stores, and let the realm rejoice.', 'Объявить амнистию, открыть амбары, пусть держава радуется.', '天下に大赦を頒ち、倉を開いて民と共に祝う。', 'Amnestie erlassen, die Vorräte öffnen und das Reich mitfreuen lassen.'),
 q('举国欢腾，列国来贺，{kingdom}一时风头无两。', '舉國歡騰，列國來賀，{kingdom}一時風頭無兩。', 'The realm cheers; foreign courts bring congratulations; {kingdom} stands unmatched in the hour.', 'Держава ликует; чужие дворы поздравляют; {kingdom} сияет, как никогда.', '挙国が沸き立ち、列国が祝いに来る——{kingdom}の栄光ことのほか。', 'Das Reich jubelt; fremde Höfe gratulieren; {kingdom} steht unvergleichlich da.'),
 q('简仪报喜', '簡儀報喜', 'A simple celebration', 'Скромно отметить', '簡素に祝う', 'Schlicht feiern'),
 q('减省礼仪，仅告庙颁赏，不扰万民。', '減省禮儀，僅告廟頒賞，不擾萬民。', 'Set aside pomp; tell the ancestors, grant modest rewards, and leave the folk in peace.', 'Без пышности: объявить храмам, дать скромные награды и не тревожить народ.', '儀礼を省き、廟に告げ恩賞を頒つのみ——民を煩わせぬ。', 'Prunk meiden; den Ahnen verkünden, bescheidene Gaben verteilen und das Volk in Ruhe lassen.'),
 q('喜讯传出，国中虽贺，未扰民生。', '喜訊傳出，國中雖賀，未擾民生。', 'The joyful word spreads; the realm congratulates, yet daily life is undisturbed.', 'Благая весть идёт; держава радуется, но жизнь не потревожена.', '吉報が伝わり国中も祝うが、民の暮らしは乱れない。', 'Die frohe Kunde geht um; das Reich freut sich, doch der Alltag bleibt unberührt.')
)
ev('heir_duel', 2,
 ('储君较技', '儲君較技', 'The Heir\'s Duel', 'Дуэль наследника', '太子の比技', 'Das Duell des Erben'),
 ('储君与亲王校场较技，观者如堵，胜负关乎储位颜面与人心。', '儲君與親王校場較技，觀者如堵，勝負關乎儲位顏面與人心。', 'The heir and a prince clash in the yard before a packed crowd; the result binds the heir\'s honor and the people\'s hearts.', 'Наследник и принц сходятся на плацу перед толпой; исход связан с честью наследника и сердцами народа.', '太子と親王が演武場で技を競う——観衆で埋まり、勝敗が儲位の面目と人心を懸ける。', 'Thronfolger und Prinz messen sich auf dem Übungsplatz vor dichtem Publikum; das Ergebnis bindet die Ehre des Erben und die Herzen des Volkes.'),
 q('任其比试', '任其比試', 'Let it run', 'Дать бою идти', '技比べに任す', 'Den Kampf laufen lassen'),
 q('不复劝止，胜负听之，任其自决。', '不復勸止，勝負聽之，任其自決。', 'No one stops it; let the outcome take its own course.', 'Не останавливать; пусть исход решится сам.', '止めずに、勝敗を天に任せる。', 'Nicht stoppen; der Ausgang nimmt seinen eigenen Lauf.'),
 q('比试失手，储君负伤，朝野流言四起。', '比試失手，儲君負傷，朝野流言四起。', 'A stroke goes wrong; the heir is hurt; rumor spills through court and country.', 'Удар срывается; наследник ранен; слухи ползут при дворе и в провинции.', '手が滑り太子が負傷——朝野に流言が四散する。', 'Ein Hieb gerät daneben; der Erbe wird verletzt; Gerüchte fließen durch Hof und Land.'),
 q('及时叫停', '及時叫停', 'Stop the bout', 'Прервать бой', '打ち止める', 'Den Kampf stoppen'),
 q('亲临校场叫停，各赐彩头。', '親臨校場叫停，各賜彩頭。', 'Step into the yard yourself, halt it, and gift both their prizes.', 'Самому выйти на плац, прервать и одарить обоих.', '自ら校場に臨んで打ち止め、両者に彩頭を賜う。', 'Selbst in den Hof treten, es stoppen und beiden Geschenke geben.'),
 q('较技化险，列国传{king}教亲有方，天伦无伤。', '較技化險，列國傳{king}教親有方，天倫無傷。', 'The bout turns safe; foreign courts say {king} teaches his kin well, and the family ties hold.', 'Дуэль обезврежена; чужие дворы говорят, что {king} умеет учить родню, и узы семьи целы.', '比技が危うくも治まり、列国は{king}の身内の導き方を讃える——天倫も傷つかず。', 'Der Kampf kommt glimpflich aus; fremde Höfe sagen, {king} erziehe seine Sippe gut, und die Familie bleibt unversehrt.')
)
ev('court_faction', 2,
 ('朝堂党争', '朝堂黨爭', 'Factional Strife', 'Распри фракций', '朝堂の党争', 'Fraktionsstreit'),
 ('两派朝臣互相攻讦，弹劾奏章如雪片飞至{king}案头，寝食难安。', '兩派朝臣互相攻訐，彈劾奏章如雪片飛至{king}案頭，寢食難安。', 'Two court factions trade accusations; memorials snow onto {king}\'s desk and rob him of sleep.', 'Две фракции осыпают друг друга обвинениями; жалобы летят на стол {king}, лишая его сна.', '二派の朝臣が互いを弾劾——上奏の雪が{king}の机に舞い、寝食も安からぬ。', 'Zwei Hofgruppen beschuldigen einander; Denkschriften schneien auf {king}\'s Tisch und rauben ihm den Schlaf.'),
 q('坐观虎斗', '坐觀虎鬥', 'Let them clash', 'Дать им грызться', '虎の闘いを観る', 'Sie kämpfen lassen'),
 q('两不相帮，坐观其变，以静制动。', '兩不相幫，坐觀其變，以靜制動。', 'Side with neither; watch the storm turn and still it by stillness.', 'Не вставать ни на чью сторону; смотреть, как всё крутится, и усмирить неподвижностью.', 'どちらにも与せず、成り行きを見守り、静をもって動を制す。', 'Auf keiner Seite stehen; den Sturm beobachten und durch Stille bezwingen.'),
 q('党争愈演愈烈，朝政几近停摆，奏章积压如山。', '黨爭愈演愈烈，朝政幾近停擺，奏章積壓如山。', 'The strife rages on; governance nearly grinds to a halt and memorials pile up like hills.', 'Распри кипят; правление почти остановилось, прошения горой навалились.', '党争が激化し、朝政はほとんど止まる——上奏が山と積もる。', 'Der Streit wütet weiter; das Regieren stockt beinahe, und die Denkschriften türmen sich.'),
 q('亲裁曲直', '親裁曲直', 'Judge it himself', 'Рассудить самому', '親ら裁く', 'Selbst entscheiden'),
 q('召两派同殿对质，亲裁是非。', '召兩派同殿對質，親裁是非。', 'Summon both factions to confront and rule on right and wrong yourself.', 'Созвать обе фракции и рассудить, кто прав.', '二派を同殿に召して対質させ、親ら曲直を裁く。', 'Beide Fraktionen zur Konfrontation rufen und über Recht und Unrecht selbst richten.'),
 q('是非既明，党争稍戢，列国称{king}御下有方。', '是非既明，黨爭稍戢，列國稱{king}御下有方。', 'Right and wrong are settled; the strife eases; foreign courts praise {king}\'s firm hand.', 'Правда установлена; распри слабеют; чужие дворы хвалят твёрдую руку {king}.', '是非が明らかになり、党争も和らぐ——列国が{king}の御し方を称ぶ。', 'Recht und Unrecht sind gesetzt; der Streit flaut ab; fremde Höfe rühmen {king}\'s feste Hand.')
)
ev('eunuch_reform', 2,
 ('清理宦籍', '清理宦籍', 'Refounding the Eunuchs', 'Реформа евнухов', '宦籍の整理', 'Neuerung im Eunuchenwesen'),
 ('内廷宦员冗滥，有宦者自请{king}清理宦籍、纳金自赎，朝议分裂。', '內廷宦員冗濫，有宦者自請{king}清理宦籍、納金自贖，朝議分裂。', 'The eunuch offices swell; some eunuchs beg {king} to purge the rolls for a payment in gold; the court splits.', 'Евнухи расплодились; часть их просит {king} провести чистку за выкуп; двор разделился.', '内廷の宦員が冗濫——宦官らが{king}に宦籍の整理と金納による自贖を自ら願い出て、朝議が割れる。', 'Der Eunuchenstab schwillt; manche Eunuchen bitten {king}, die Listen für Gold zu säubern; der Hof teilt sich.'),
 q('纳金自赎', '納金自贖', 'Buy the post back', 'Выкупить должность', '金で職を買う', 'Die Stelle loskaufen'),
 q('令冗宦纳金自赎，量才留用。', '令冗宦納金自贖，量才留用。', 'Let the surplus redeem themselves with gold; keep the able.', 'Пусть лишние выкупятся золотом; способных оставить.', '冗宦に金を納めて自贖させ、才ある者だけを留める。', 'Die Überzähligen sich mit Gold lösen lassen; die Tüchtigen behalten.'),
 q('宦籍一新，赎金入库，国库为之充盈。', '宦籍一新，贖金入庫，國庫為之充盈。', 'The rolls are renewed; the ransoms pour in and the vaults fill.', 'Списки обновлены; выкупы текут; казна полнится.', '宦籍が一新し、贖金が庫に入って満ちる。', 'Die Listen sind erneuert; die Lösegelder fließen ein, und die Kassen füllen sich.'),
 q('痛斥其议', '痛斥其議', 'Rebuke the plea', 'Осадить просителей', '議を斥ける', 'Die Bitte zurückweisen'),
 q('斥其为市贾之行，断然驳回。', '斥其為市賈之行，斷然駁回。', 'Call it huckstering and refuse outright.', 'Назвать торгашеством и отказать наотрез.', '市買いの行いと罵り、断とて退ける。', 'Es als Krämertum schelten und rundweg ablehnen.'),
 q('驳回之讯传出，阉党抱团自保，宫中暗斗。', '駁回之訊傳出，閹黨抱團自保，宮中暗鬥。', 'The refusal spreads; the eunuch faction closes ranks and the palace bickers behind closed doors.', 'Отказ разнёсся; евнухи сомкнулись; дворец грызётся за спинами.', '却下の報が広がり、閹党が結束して自保——宮中で暗闘。', 'Die Ablehnung spricht sich herum; die Eunuchen rücken zusammen, der Palast zankt hinter verschlossenen Türen.')
)
ev('queen_rescript', 2,
 ('王后懿旨', '王后懿旨', 'The Queen\'s Rescript', 'Рескрипт королевы', '王妃の懿旨', 'Der Erlass der Königin'),
 ('王后以灾民待哺为由，请{king}准其开仓赈济，言官则讥后宫干政。', '王后以災民待哺為由，請{king}准其開倉賑濟，言官則譏後宮干政。', 'The queen pleads for {king} to let her open the granaries for the starving; censors sneer at a consort ruling.', 'Королева просит {king} дозволить ей открыть амбары для голодных; цензоры фыркают на правление из-за занавеса.', '王妃が災民の飢えを理由に、{king}に開倉賑済を請う——言官は後宮の干政と嘲る。', 'Die Königin bittet {king} um die Erlaubnis, die Kornkammern für die Hungernden zu öffnen; Zensoren spotten über Herrschaft aus der Kammer.'),
 q('准旨开仓', '准旨開倉', 'Grant the rescript', 'Разрешить', '旨を許す', 'Den Erlass gestatten'),
 q('准其开仓放粮，赈济灾民。', '准其開倉放糧，賑濟災民。', 'Let her open the stores and feed the starving.', 'Дозволить открыть амбары и накормить голодных.', '倉を開いて放糧し、災民を救済する。', 'Ihr erlauben, die Vorräte zu öffnen und die Hungernden zu speisen.'),
 q('灾民得济，列国颂{king}夫妇同心，仁声彻于邻境。', '災民得濟，列國頌{king}夫婦同心，仁聲徹於鄰境。', 'The starving are saved; foreign courts praise {king} and his queen as one, and the word of their kindness crosses every border.', 'Голодные спасены; чужие дворы славят {king} и королеву как одно, и весть о доброте пересекает все границы.', '災民が救われ、列国は{king}夫妻の同心を讃える——仁の声が隣境にまで届く。', 'Die Hungernden sind gerettet; fremde Höfe rühmen {king} und Königin als eines Sinnes, und die Kunde ihrer Güte überquert alle Grenzen.'),
 q('谕止其行', '諭止其行', 'Veto it', 'Запретить', '行いを止める', 'Untersagen'),
 q('谕以宫闱不得预政，止其开仓。', '諭以宮闈不得預政，止其開倉。', 'Tell her the chamber stays out of rule; no stores.', 'Сказать, что за занавесом не правят; амбары закрыты.', '宮闈は政を預からずと諭し、開倉を止めさせる。', 'Ihr sagen, die Kammer regiere nicht; keine Vorräte.'),
 q('王后怏怏，宫闱与言官各怀芥蒂。', '王后怏怏，宮闈與言官各懷芥蒂。', 'The queen sulks; chamber and censors each nurse their grudge.', 'Королева дуется; за занавесом и среди цензоров — взаимные обиды.', '王妃が不機嫌に——宮闈と言官が互いに芥蒂を抱く。', 'Die Königin schmollt; Kammer und Zensoren hegen ihren Groll.')
)

# ===== 忠奸、细作与嗣子 =====
ev('loyalist_swear', 2,
 ('孤臣表忠', '孤臣表忠', 'The Loyalist\'s Vow', 'Клятва верного слуги', '忠臣の誓い', 'Der Treueschwur'),
 ('御史大夫求见，愿以重誓表忠，望{king}许其为辅政之首，以肃朝纲。', '御史大夫求見，願以重誓表忠，望{king}許其為輔政之首，以肅朝綱。', 'The grand censor seeks audience to swear his loyalty and asks {king} to make him the foremost adviser, so the court will be put in order.', 'Великий цензор просит аудиенции, чтобы поклясться в верности и попросить {king} сделать его первым советником и навести порядок при дворе.', '御史大夫が謁見を請い、重誓を立てて忠を示し、{king}に輔政の首となって朝綱を粛正せんことを願う。', 'Der Großzensor bittet um Audienz, schwört seine Treue und bittet {king}, ihn zum ersten Ratgeber zu machen, damit der Hof geordnet sei.'),
 q('纳其重誓', '納其重誓', 'Accept the vow', 'Принять клятву', '誓いを受く', 'Den Schwur annehmen'),
 q('当殿纳其重誓，委以辅政之任。', '當殿納其重誓，委以輔政之任。', 'Receive the oath in hall and entrust him with the charge of counsel.', 'Принять клятву в зале и доверить заботу о совете.', '殿上でその重誓を受け、輔政の任を委ねる。', 'Den Schwur im Saal empfangen und ihm das Amt des Rates anvertrauen.'),
 q('忠臣得用，朝野称庆，列国闻之亦敬。', '忠臣得用，朝野稱慶，列國聞之亦敬。', 'The loyal man is put to use; court and country rejoice; foreign courts hold it worthy.', 'Верный слуга в деле; двор и держава радуются; чужие дворы признают.', '忠臣が用いられ、朝野が慶ぶ——列国もこれを敬う。', 'Der Treue ist im Amt; Hof und Land frohlocken; fremde Höfe halten es für würdig.'),
 q('嘉之而已', '嘉之而已', 'Praise, no more', 'Похвалить и всё', '讃えるのみ', 'Nur loben'),
 q('温言嘉勉，不授首辅之任。', '溫言嘉勉，不授首輔之任。', 'Warm praise, but no seat at the head of the council.', 'Тёплая похвала — но не место во главе совета.', '温言で讃え励ますが、首輔の任は授けない。', 'Warmes Lob, aber kein Sitz an der Spitze des Rates.'),
 q('御史大夫再拜而退，暂无波澜，然其心未已。', '御史大夫再拜而退，暫無波瀾，然其心未已。', 'The censor bows twice and withdraws; for now no wave stirs, yet his heart is not still.', 'Цензор дважды кланяется и уходит; волны нет, но сердце его неспокойно.', '御史大夫が再拝して退く——当面は波立たず、だがその志は尽きぬ。', 'Der Zensor verneigt sich zweimal und zieht ab; vorerst rührt sich keine Welle, doch sein Herz ist nicht still.')
)
ev('spy_in_palace', 2,
 ('宫中细作', '宮中細作', 'A Spy in the Palace', 'Лазутчик во дворце', '宮中の間者', 'Ein Spion im Palast'),
 ('密探查知：宫中藏有他国细作，已潜伏多年，手眼通天。', '密探查知：宮中藏有他國細作，已潛伏多年，手眼通天。', 'The secret service finds a foreign spy long hidden in the palace, with eyes and hands everywhere.', 'Тайная служба нашла чужеземного лазутчика, годы скрывавшегося во дворце с глазами и руками повсюду.', '密探が知る——宮中に他国の間者が長年潜み、手眼が宮を貫いている。', 'Der Geheimdienst findet einen fremden Spion, der seit Jahren im Palast lauert, mit Augen und Händen überall.'),
 q('将计就计', '將計就計', 'Turn the spy', 'Использовать лазутчика', '計を返す', 'Den Spion umdrehen'),
 q('佯作不知，反喂假情报，放长线钓大鱼。', '佯作不知，反餵假情報，放長線釣大魚。', 'Feign ignorance, feed him false word; let the line run long.', 'Делать вид, что не знаешь, и подкидывать дезу — длинная леска.', '知らぬふりをして偽情報を流し、長い糸で大物を釣る。', 'Unwissenheit heucheln, ihm Falsches füttern; die Schnur lang laufen lassen.'),
 q('假情报已入其手，彼国益轻{kingdom}——只待来年收网。', '假情報已入其手，彼國益輕{kingdom}——只待來年收網。', 'The false word is in his hands; that court grows careless of {kingdom}; the net awaits next year.', 'Деза в его руках; тот двор легкомысленно относится к {kingdom}; сеть ждёт будущего года.', '偽情報は彼の手に——その国も{kingdom}を侮る。網は来年に備わる。', 'Die falsche Kunde ist in seiner Hand; jener Hof dünkt sich über {kingdom} erhaben; das Netz wartet aufs nächste Jahr.'),
 q('立时搜捕', '立時搜捕', 'Raid at once', 'Немедля обыскать', '直ちに捜索', 'Sofort durchsuchen'),
 q('封锁宫门，按图索骥，务必擒获。', '封鎖宮門，按圖索驥，務必擒獲。', 'Bar the gates, follow the leads, and catch him.', 'Запереть ворота, идти по следу и поймать.', '宮門を封鎖し、手掛かりを辿って必ず捕捉する。', 'Die Tore sperren, den Spuren folgen und ihn fangen.'),
 q('宫中大索数日，人人自危，细作却似人间蒸发。', '宮中大索數日，人人自危，細作卻似人間蒸發。', 'Days of sweeping the palace; all tremble, yet the spy seems to have vanished from the earth.', 'Дни обысков; все трепещут, но лазутчик словно испарился.', '宮中の大捜索が数日続き、皆が自らを危ぶむ——間者は忽然と消えたよう。', 'Tage des Durchsuchens; alle beben, doch der Spion scheint vom Erdboden verschwunden.')
)
ev('spy_exposure', 2,
 ('细作败露', '細作敗露', 'The Spy Exposed', 'Лазутчик раскрыт', '間者の露見', 'Der Spion entlarvt'),
 ('收网之时已至——被喂假的细作仓皇欲遁，其国使节此刻正来探问。', '收網之時已至——被餵假的細作倉皇欲遁，其國使節此刻正來探問。', 'Time to close the net; the duped spy scrambles to flee, even as his nation\'s envoy comes asking.', 'Пора закрыть сеть; обманутый лазутчик рвётся бежать, а посол его державы как раз прибыл с расспросами.', '網を打つ時——偽情報を食わされた間者が慌てて遁げんとす。その国の使節が此刻、問い尋ねに来る。', 'Zeit, das Netz zu schließen; der genasführte Spion sucht zu fliehen, während der Gesandte seiner Nation zu fragen kommt.'),
 q('明正典刑', '明正典刑', 'Execute by law', 'Казнить по закону', '公開の処刑', 'Nach Recht hinrichten'),
 q('枭首示众，以儆效尤，肃清宫闱。', '梟首示眾，以儆效尤，肅清宮闈。', 'Hang his head high as a stern warning and cleanse the chamber.', 'Выставить голову как суровое предостережение и очистить покои.', '首を曝して見せしめとし、宮闈を粛清する。', 'Den Kopf zur Schau stellen, als hartes Zeichen, und die Kammer säubern.'),
 q('细作伏诛，然其国震怒，朝野议论未息。', '細作伏誅，然其國震怒，朝野議論未息。', 'The spy dies, but that court burns with rage and the talk here does not settle.', 'Лазутчик казнён, но тот двор пылает гневом, и толки не утихают.', '間者は誅されたが、その国は激怒——朝野の議論は収まらない。', 'Der Spion stirbt, doch jener Hof kocht vor Zorn, und das Gerede legt sich nicht.'),
 q('押解出境', '押解出境', 'Hand him back', 'Выдворить', '国外へ送還', 'Ihn zurückschicken'),
 q('释而不杀，厚送出境，以全邦交。', '釋而不殺，厚送出境，以全邦交。', 'Spare his life, send him richly home to keep the peace.', 'Пощадить, щедро отправить домой — сохранить мир.', '殺さずに釈放し、厚く送り出して国交を全うする。', 'Ihn schonen, reich beschenkt heimschicken, um den Frieden zu wahren.'),
 q('细作归国，彼国赧然，列国赞{kingdom}有国士之风。', '細作歸國，彼國赧然，列國讚{kingdom}有國士之風。', 'The spy goes home; that court is shamed, and foreign courts praise {kingdom}\'s gracious might.', 'Лазутчик отбыл; тот двор посрамлён; чужие дворы славят великодушие {kingdom}.', '間者が帰国し、彼国は汗顔——列国が{kingdom}の国士の風を讃える。', 'Der Spion kehrt heim; jener Hof schämt sich; fremde Höfe rühmen {kingdom}\'s großmütige Stärke.')
)
ev('crown_jewels', 2,
 ('御宝失当', '御寶失當', 'The Pawned Crown Jewels', 'Заложенные королевские сокровища', '御宝の質入れ', 'Die verpfändeten Kronjuwelen'),
 ('御用珠宝竟现身当铺，追查之下，盗当者直指{king}的内廷亲贵。', '御用珠寶竟現身當鋪，追查之下，盜當者直指{king}的內廷親貴。', 'Royal jewels turn up in a pawnshop; the trail points to a kinsman of {king}\'s inner court.', 'Королевские сокровища всплыли в ломбарде; след ведёт к родне внутреннего двора {king}.', '御用の宝飾が質屋に現れる——追いかければ、盗当の手は{king}の内廷の親貴を指す。', 'Königliche Juwelen tauchen im Pfandleihhaus auf; die Spur führt zu einem Verwandten des Innenhofs von {king}.'),
 q('赎回入库', '贖回入庫', 'Redeem the jewels', 'Выкупить', '買い戻す', 'Die Juwelen auslösen'),
 q('急赎御物归库，内情暂抑。', '急贖御物歸庫，內情暫抑。', 'Redeem the treasure quickly and hush the matter.', 'Срочно выкупить сокровище и замять.', '急いで御物を買い戻し庫に納め、内情はひとまず抑える。', 'Den Schatz rasch auslösen und die Sache vertuschen.'),
 q('宝饰归库，库藏却空了大半，岁内难复。', '寶飾歸庫，庫藏卻空了大半，歲內難復。', 'The jewels return, yet the vault stands far emptier and will not recover this year.', 'Сокровища вернулись, но казна опустела наполовину и не восстановится до конца года.', '宝飾は庫に戻るが、庫は大半空になり、年内に回復は難しい。', 'Die Juwelen kehren zurück, doch die Kassen stehen weit leerer da und erholen sich nicht vor Jahresende.'),
 q('穷究盗当', '窮究盜當', 'Track the thief', 'Найти вора', '盗当を追及', 'Den Dieb verfolgen'),
 q('锁拿经手之人，严刑究办。', '鎖拿經手之人，嚴刑究辦。', 'Arrest all hands involved and question them hard.', 'Арестовать причастных и допросить жёстко.', '手を触れた者を捕らえ、厳しく究める。', 'Alle Beteiligten festnehmen und hart verhören.'),
 q('牵出宫中一串旧案，亲贵人人自危。', '牽出宮中一串舊案，親貴人人自危。', 'Old palace offenses surface in a string; every noble kin fears for himself.', 'Всплывают старые дворцовые грехи; каждая родня трепещет.', '宮中の旧案が連鎖して暴かれ、親貴は皆自らを危ぶむ。', 'Alte Palastvergehen kommen in einer Reihe ans Licht; jeder Vornehme bangt um sich.')
)
ev('hunting_pact', 2,
 ('共猎盟约', '共獵盟約', 'A Hunting Pact', 'Охотничий пакт', '共狩りの盟', 'Ein Jagdpakt'),
 ('邻国邀{king}共猎于界山，欲借此议定互市与边境之约，似有诚意。', '鄰國邀{king}共獵於界山，欲藉此議定互市與邊境之約，似有誠意。', 'A neighbor invites {king} to hunt the border hills together, hoping to settle trade and frontier terms; the offer seems sincere.', 'Сосед приглашает {king} на совместную охоту в пограничных холмах, надеясь договориться о торговле и границе; предложение кажется искренним.', '隣国が{king}を界山での共狩りに招く——互市と国境の盟約を議せんと。その意は誠のようだ。', 'Ein Nachbar lädt {king} zur gemeinsamen Jagd auf die Grenzhügel, um Handel und Grenzabkommen zu regeln; die Einladung wirkt aufrichtig.'),
 q('欣然共猎', '欣然共獵', 'Join the hunt', 'Принять приглашение', '共に狩る', 'Mitjagen'),
 q('率亲卫赴约，猎后议定条约。', '率親衛赴約，獵後議定條約。', 'Ride out with your guards; talk terms after the chase.', 'Выехать со стражей; говорить о делах после охоты.', '親衛を率いて約を果たし、狩りの後に条約を議する。', 'Mit der Leibwache aufbrechen; nach der Jagd die Bedingungen beraten.'),
 q('一猎尽欢，条约既定，邻好之谊更笃。', '一獵盡歡，條約既定，鄰好之誼更篤。', 'The hunt is joyfully done, the treaty signed, and neighborly bonds bind tighter.', 'Охота в радость; договор подписан; соседские узы крепче.', '狩りは大いに楽しまれ、条約は定まり、隣の誼がいっそう深まる。', 'Die Jagd endet in Freude, der Vertrag steht, und die Nachbarschaft bindet fester.'),
 q('借辞推拒', '借辭推拒', 'Decline politely', 'Вежливо отказать', '辞退する', 'Höflich ablehnen'),
 q('以时令不宜为由，遣使谢绝。', '以時令不宜為由，遣使謝絕。', 'Plead the season and send word of refusal.', 'Сослаться на сезон и прислать отказ.', '時節を理由に使節を遣わして断る。', 'Die Jahreszeit vorschützen und absagen lassen.'),
 q('邻国未再提起，围猎之事作罢，边境照旧。', '鄰國未再提起，圍獵之事作罷，邊境照舊。', 'The neighbor does not raise it again; the hunt is off, and the frontier stays as it was.', 'Сосед больше не поднимает; охота отменена, граница прежняя.', '隣国も再び取り上げず、狩りは沙汰止み——国境は従来通り。', 'Der Nachbar nimmt es nicht wieder auf; die Jagd ist abgeblasen, die Grenze bleibt, wie sie war.')
)
ev('fortune_teller', 2,
 ('相士之言', '相士之言', 'The Fortune Teller', 'Гадалка', '相士の言葉', 'Der Wahrsager'),
 ('相士求见，谓观{king}之相，来年必有大旱，须早作禳解，否则空仓。', '相士求見，謂觀{king}之相，來年必有大旱，須早作禳解，否則空倉。', 'A fortune teller begs audience and reads {king}\'s face: next year comes a great drought; avert it early or the granaries will stand empty.', 'Гадалка просит аудиенции и читает лицо {king}: впереди великая засуха; отврати её заранее, иначе амбары опустеют.', '相士が謁見を請う——{king}の相を見るに、来年必ず大旱あり、早く禳うべき、さもなくば倉が空になると。', 'Ein Wahrsager bittet um Audienz und liest {king}\'s Antlitz: nächstes Jahr droht große Dürre; ihr früh wehren, sonst stehen die Speicher leer.'),
 q('重金问吉', '重金問吉', 'Consult him generously', 'Щедро спросить', '重金で占う', 'Reichlich fragen'),
 q('重金礼聘，依其言设坛祈雨。', '重金禮聘，依其言設壇祈雨。', 'Pay him well and raise an altar to pray for rain, as he says.', 'Щедро заплатить и поднять алтарь для молитв о дожде.', '重金を聘し、言う通りに壇を設けて雨を祈る。', 'Gut bezahlen und, wie er rät, einen Altar für Regengebete errichten.'),
 q('坛成之日，列国皆闻{king}敬天恤民。', '壇成之日，列國皆聞{king}敬天恤民。', 'The altar rises; foreign courts hear how {king} reveres heaven and pities his people.', 'Алтарь встал; чужие дворы слышат, как {king} чтит небо и жалеет народ.', '壇が成り、列国が{king}の天を敬い民を恤むを聞く。', 'Der Altar steht; fremde Höfe hören, wie {king} den Himmel ehrt und das Volk beweint.'),
 q('斥其妖言', '斥其妖言', 'Scold and shun', 'Изгнать', '妖言と斥す', 'Verweisen'),
 q('斥为妖言惑众，驱出宫门。', '斥為妖言惑眾，驅出宮門。', 'Call it sorcery-talk and cast him from the gates.', 'Назвать колдовскими побасёнками и выставить за ворота.', '妖言惑衆と罵り、宮門から追い出す。', 'Als Zauberei-Gerede schelten und aus den Toren werfen.'),
 q('相士执意预言，流言愈传愈盛，人心浮动。', '相士執意預言，流言愈傳愈盛，人心浮動。', 'The fortune teller insists on the prophecy; the rumor swells and hearts waver.', 'Гадалка стоит на своём; молва лишь ширится, сердца колеблются.', '相士は予言を譲らず、流言はますます盛んに——人心が動く。', 'Der Wahrsager beharrt auf der Prophezeiung; das Gerücht schwillt, und die Herzen wanken.')
)
ev('princess_guard', 2,
 ('公主仪卫', '公主儀衛', 'The Princess\'s Guard', 'Стража принцессы', '公主の護衛', 'Die Garde der Prinzessin'),
 ('公主出阁在即，礼官请{king}扩公主仪卫，以壮排场，备节库之臣反对。', '公主出閣在即，禮官請{king}擴公主儀衛，以壯排場，備節庫之臣反對。', 'The princess\'s wedding nears; the rite-master asks {king} to enlarge her guard for the grand procession, and the treasury keeper objects.', 'Свадьба принцессы близко; церемониймейстер просит {king} расширить её стражу для пышного кортежа, а казначей возражает.', '公主の輿入れが迫る——礼官が{king}に儀衛を増やし威儀を張ることを願うが、金を節する臣が反対する。', 'Die Hochzeit der Prinzessin naht; der Zeremonienmeister bittet {king}, ihre Garde für den prunkvollen Zug zu erweitern; der Schatzmeister widerspricht.'),
 q('增置仪卫', '增置儀衛', 'Enlarge the guard', 'Расширить стражу', '儀衛を増す', 'Die Garde erweitern'),
 q('拨金增置车马护卫，以壮威仪。', '撥金增置車馬護衛，以壯威儀。', 'Fund extra chariots and guards to give the procession splendor.', 'Дать деньги на колесницы и охрану для блеска процессии.', '金を撥し車馬と護衛を増やし、威儀を壮にする。', 'Wagen und Wachen finanzieren, um dem Zug Glanz zu geben.'),
 q('仪卫整肃，公主出阁风仪可观，路人称叹。', '儀衛整肅，公主出閣風儀可觀，路人稱嘆。', 'The guard stands in order; the princess leaves in proper splendor, and folk along the road admire.', 'Стража в порядке; принцесса покидает дворец в достойном блеске, и люди восхищаются.', '儀衛が整い、公主の輿入れは見事な威儀——道中の人々が嘆声を漏らす。', 'Die Garde steht in Ordnung; die Prinzessin zieht aus geziemender Pracht, und die Leute bewundern.'),
 q('循例而从', '循例而從', 'Keep the old quota', 'По обычаю', '例に従う', 'Beim alten Umfang bleiben'),
 q('只循旧例，不增仪卫，以节库帑。', '只循舊例，不增儀衛，以節庫帑。', 'Follow the old rule, enlarge nothing, and spare the vaults.', 'Следовать обычаю, ничего не расширять и щадить казну.', '旧例にのみ従い、儀衛を増やさず庫を節する。', 'Der alten Regel folgen, nichts erweitern und die Kassen schonen.'),
 q('仪卫如旧，礼成而返，诸事无虞。', '儀衛如舊，禮成而返，諸事無虞。', 'The guard stays as it was; the rites complete, all return, and nothing is amiss.', 'Стража прежняя; обряды свершились, все вернулись, всё в порядке.', '儀衛は従来通り、礼を終えて皆戻る——万事恙なし。', 'Die Garde bleibt wie gehabt; die Riten enden, alle kehren zurück, und nichts fehlt.')
)
ev('noble_trial', 2,
 ('贵族受审', '貴族受審', 'A Noble on Trial', 'Суд над аристократом', '貴族の裁判', 'Ein Adliger vor Gericht'),
 ('世袭伯爵强占民田，百姓呼冤，案卷已呈{king}案头，宗室为之求情。', '世襲伯爵強佔民田，百姓呼冤，案卷已呈{king}案頭，宗室為之求情。', 'A hereditary earl seized farmland and the peasants cry out; the case file lies before {king}, and kinsmen beg for mercy.', 'Наследственный граф захватил крестьянские поля, крестьяне вопиют; дело лежит перед {king}, и родня просит пощады.', '世襲の伯爵が民田を強奪し、百姓が冤を訴える——案巻は{king}の机に上がり、宗室がその為に情けを請う。', 'Ein Erbgraf hat Bauernland geraubt, und die Bauern schreien um Recht; die Akte liegt vor {king}, und Verwandte flehen um Gnade.'),
 q('依律严惩', '依律嚴懲', 'Judge by law', 'Судить по закону', '法に照らす', 'Nach Recht richten'),
 q('强占者依律问罪，田产充公。', '強佔者依律問罪，田產充公。', 'Punish the seizure by law and confiscate the lands.', 'Наказать захват по закону и конфисковать земли.', '強奪の罪を法に照らし、田産を没収する。', 'Die Raubtat nach Gesetz strafen und die Ländereien einziehen.'),
 q('伯爵下狱，宗室侧目，谣诼四起。', '伯爵下獄，宗室側目，謠諑四起。', 'The earl falls; noble kin glare sidelong; slander rises on all sides.', 'Граф пал; родня косится; клевета растёт.', '伯爵が下獄し、宗室が目をそらし、噂が四方に立つ。', 'Der Graf fällt; Verwandte schielen; Verleumdung steigt ringsum.'),
 q('罚金折罪', '罰金折罪', 'Fine and forgive', 'Штраф и прощение', '罰金で罪を折る', 'Buße statt Strafe'),
 q('命其返田纳金，从轻发落。', '命其返田納金，從輕發落。', 'Order the fields returned and a fine paid; lend the punishment less weight.', 'Велеть вернуть поля и заплатить штраф; наказание легче.', '田を返させ金を納めさせ、軽く扱う。', 'Felder zurückgeben und Strafe zahlen lassen; das Urteil mildern.'),
 q('民田得还，列国亦称{king}宽严有度。', '民田得還，列國亦稱{king}寬嚴有度。', 'The fields return; foreign courts say {king} tempers justice with mercy.', 'Поля вернулись; чужие дворы говорят, что {king} смягчает правосудие милостью.', '民田が還り、列国も{king}の寛厳の度を称ぶ。', 'Die Felder kehren zurück; fremde Höfe sagen, {king} mische Gerechtigkeit mit Milde.')
)
ev('jester_reign', 2,
 ('弄臣当道', '弄臣當道', 'The Jester\'s Favor', 'Фаворит-шут', '弄臣の寵', 'Die Gunst des Narren'),
 ('弄臣日日随侍{king}左右，谗言与笑话并出，近臣多所不满。', '弄臣日日隨侍{king}左右，讒言與笑話並出，近臣多所不滿。', 'The jester attends {king} daily, gossip and jokes in one breath; the inner circle is much displeased.', 'Шут день и ночь при {king}, сплетни и шутки в одном духе; ближний круг недоволен.', '弄臣が日々{king}の傍らに伺候し、讒言と冗談を並べる——近臣の不満が募る。', 'Der Narr ist täglich bei {king}, Klatsch und Späße in einem Atem; die Umgebung ist verdrossen.'),
 q('纵其嬉游', '縱其嬉遊', 'Indulge him', 'Потакать', '放っておく', 'Ihn gewähren lassen'),
 q('怜其有趣，纵容而已，不令预政。', '憐其有趣，縱容而已，不令預政。', 'He is amusing; let him be, but keep him from rule.', 'Он забавен; пусть будет, но не у власти.', '面白いと憐れんで、そのままに——ただし政には関与させぬ。', 'Er ist unterhaltsam; man lässt ihn, hält ihn aber von der Herrschaft fern.'),
 q('弄臣出入六宫如入无人之境，朝野侧目。', '弄臣出入六宮如入無人之境，朝野側目。', 'The jester comes and goes through the six courts unhindered; court and land glare sidelong.', 'Шут ходит по шести покоям без помех; двор и держава косятся.', '弄臣が六宮を無人の如く出入りし、朝野が横目で見る。', 'Der Narr geht durch alle sechs Gemächer ungehindert; Hof und Land schielen.'),
 q('稍加约束', '稍加約束', 'Reel him in', 'Приструнить', '抑える', 'Ihn zügeln'),
 q('限其出入，赏罚由己，稍正宫规。', '限其出入，賞罰由己，稍正宮規。', 'Limit his comings and goings, keep reward and rebuke in hand, and tidy the court rules.', 'Ограничить его хождения, награда и упрёк в своей руке, и привести в порядок правила двора.', '出入りの範囲を限り、賞罰を己の手に——宮規を少し正す。', 'Seine Wege beschränken, Lohn und Tadel in eigener Hand, und die Hofregeln ordnen.'),
 q('弄臣收敛，宫中玩笑依旧，未生波澜。', '弄臣收斂，宮中玩笑依舊，未生波瀾。', 'The jester reins in; the court jokes on, and no wave stirs.', 'Шут притих; двор шутит по-прежнему; волны нет.', '弄臣が控えめになり、宮中の笑いは従来通り——波立たず。', 'Der Narr mäßigt sich; der Hof scherzt weiter, und keine Welle rührt sich.')
)
ev('ancestral_rites', 2,
 ('祭祀先祖', '祭祀先祖', 'Rites for the Ancestors', 'Обряд предкам', '先祖の祭祀', 'Der Ahnenkult'),
 ('祭祖大典将近，礼官请备三牲九醢，其费不赀，度支之臣力争简省。', '祭祖大典將近，禮官請備三牲九醢，其費不貲，度支之臣力爭簡省。', 'The ancestral rite nears; the rite-master asks for the three beasts and nine sauces at no small cost, while the budget keeper fights for thrift.', 'Обряд предков близок; церемониймейстер просит трёх зверей и девять приправ — немалый расход, и казначей борется за экономию.', '祭祖の大典が近づく——礼官が三牲九醢の準備を願い出る。その費は莫大で、度支の臣が簡省を主張する。', 'Der Ahnenritus naht; der Zeremonienmeister bittet um die drei Opfertiere und neun Saucen — kein kleiner Preis, und der Schatzmeister kämpft für Sparsamkeit.'),
 q('备礼致祭', '備禮致祭', 'Offer in state', 'Полноценный обряд', '礼を尽くす', 'In voller Pracht opfern'),
 q('依礼备办，大祭三日，以报祖德。', '依禮備辦，大祭三日，以報祖德。', 'Prepare by the rite; three days of solemn offering to requite the ancestors.', 'Приготовить по обряду; три дня торжественных жертв в благодарность предкам.', '礼に依って整え、三日の大祭を行い祖徳に報いる。', 'Nach Sitte rüsten; drei Tage feierlichen Opfergangs, den Ahnen zu danken.'),
 q('祖庙香烟缭绕，列国皆称{king}不忘其本。', '祖廟香煙繚繞，列國皆稱{king}不忘其本。', 'Incense curls over the ancestral hall; foreign courts say {king} keeps his root.', 'Благовония вьются над храмом предков; чужие дворы говорят, что {king} помнит свои корни.', '祖廟に香煙が立ちこめ、列国が{king}はその本を忘れぬと称ぶ。', 'Weihrauch kräuselt über der Ahnenhalle; fremde Höfe sagen, {king} behalte seine Wurzeln.'),
 q('简祭省费', '簡祭省費', 'A modest rite', 'Скромный обряд', '簡素に祭る', 'Ein bescheidener Ritus'),
 q('减省牲醴，洁身致祭，诚心而已。', '減省牲醴，潔身致祭，誠心而已。', 'Trim the offerings; a pure-hearted rite is enough.', 'Урезать дары; обряда от чистого сердца достаточно.', '牲醴を省き、身を清めて祭る——誠心あれば足りる。', 'Die Gaben kürzen; ein Ritus reinen Herzens genügt.'),
 q('简祭既行，诸事平静，唯仪官不悦。', '簡祭既行，諸事平靜，唯儀官不悅。', 'The modest rite holds; all stays calm, only the rite-master looks displeased.', 'Скромный обряд прошёл; всё спокойно, лишь церемониймейстер недоволен.', '簡祭が行われ、万事穏やか——ただ儀官の不満が残る。', 'Der bescheidene Ritus geht vonstatten; alles bleibt still, nur der Zeremonienmeister ist ungehalten.')
)
ev('royal_soothsayer', 2,
 ('国师卜年', '國師卜年', 'The Soothsayer\'s Word', 'Слово прорицателя', '国師の卜', 'Das Wort des Wahrsagers'),
 ('国师上言：金秋将有大变，唯独大赦可解，请{king}圣裁，勿疑民言。', '國師上言：金秋將有大變，唯獨大赦可解，請{king}聖裁，勿疑民言。', 'The soothsayer warns of great change in the autumn that only a general amnesty can avert; he begs {king} to decide and heed the folk.', 'Прорицатель предупреждает о великой перемене осенью, отвратить которую способна лишь амнистия; он просит решения {king} и внимать народу.', '国師が言上す——金秋に大変あり、大赦のみこれを解くと。{king}の聖裁を請い、民の声を疑うなと。', 'Der Wahrsager warnt vor großer Veränderung im Herbst, die nur eine Generalamnestie abwenden könne; er bittet {king} zu entscheiden und das Volk zu hören.'),
 q('顺言大赦', '順言大赦', 'Follow the counsel', 'Внять пророчеству', '言う通りに赦す', 'Dem Rat folgen'),
 q('依其言颁诏大赦，祈消灾祸。', '依其言頒詔大赦，祈消災禍。', 'Issue the amnesty as he bids, praying the ill is warded off.', 'Огласить амнистию, как он велит, молясь, что беда отведена.', '言う通りに大赦の詔を頒ち、災いの消えんことを祈る。', 'Die Amnestie erlassen, wie er rät, und beten, dass das Unheil weicht.'),
 q('大赦既颁，万民感戴，列国传{king}之仁。', '大赦既頒，萬民感戴，列國傳{king}之仁。', 'The amnesty falls; the people are grateful; foreign courts carry news of {king}\'s mercy.', 'Амнистия оглашена; народ благодарен; чужие дворы трубят о милосердии {king}.', '大赦が頒ばれ、万民が感戴——列国は{king}の仁を伝える。', 'Die Amnestie ist erlassen; das Volk dankt; fremde Höfe tragen Kunde von {king}\'s Barmherzigkeit.'),
 q('斥其妄言', '斥其妄言', 'Denounce the omen', 'Отвергнуть знамение', '妄言と斥す', 'Das Omen verwerfen'),
 q('斥其荧惑人心，罢其国师之职。', '斥其熒惑人心，罷其國師之職。', 'Call it delusion of the people and strip his office.', 'Назвать мороком и снять с должности.', '人心を惑わすと罵り、国師の職を罷める。', 'Es Blendung des Volkes nennen und ihm das Amt nehmen.'),
 q('国师被逐，民间议论更炽，如星火蔓延。', '國師被逐，民間議論更熾，如星火蔓延。', 'The soothsayer is cast out; talk among the folk blazes like spreading sparks.', 'Прорицатель изгнан; толки в народе горят, как разлетающиеся искры.', '国師が追われ、民間の議論が火がついたように燃え広がる。', 'Der Wahrsager wird verstoßen; das Gerede im Volk brennt wie fliegende Funken.')
)
ev('heir_foreign', 2,
 ('异邦王储', '異邦王儲', 'The Foreign-Born Heir', 'Наследник из-за моря', '異邦の王儲', 'Der Erbe aus der Fremde'),
 ('流落异国的先王血脉寻归故土，请求{king}认归入籍，重臣意见不一。', '流落異國的先王血脈尋歸故土，請求{king}認歸入籍，重臣意見不一。', 'A bloodline of the late king, raised in foreign lands, returns home and begs {king} to restore his name; the ministers divide.', 'Кровь прежнего короля, выросшая за морем, вернулась домой и просит {king} признать её имя; советники разделились.', '異国で育った先王の血筋が、故土に帰り{king}に帰籍を請う——重臣の意見は割れる。', 'Ein Blut des alten Königs, in der Fremde aufgewachsen, kehrt heim und bittet {king} um Anerkennung; die Minister teilen sich.'),
 q('认归入籍', '認歸入籍', 'Welcome him home', 'Признать его', '帰籍を認める', 'Heimat geben'),
 q('验明正身，迎归宗庙，告于先王。', '驗明正身，迎歸宗廟，告於先王。', 'Verify his blood, bring him to the ancestral hall, and tell the late king.', 'Удостоверить кровь, ввести в храм предков и возвестить прежнему королю.', '身元を確かめ、宗廟に迎え入れ、先王に告げる。', 'Seine Blutlinie prüfen, ihn in die Ahnenhalle holen und dem alten König berichten.'),
 q('王子归宗，列国称{kingdom}重血胤之义。', '王子歸宗，列國稱{kingdom}重血胤之義。', 'The prince returns to the line; foreign courts praise {kingdom}\'s regard for kin and blood.', 'Принц вернулся в род; чужие дворы славят почитание {kingdom} крови и родства.', '王子が宗に入り、列国が{kingdom}の血族を重んずる義を称ぶ。', 'Der Prinz kehrt ins Geschlecht zurück; fremde Höfe rühmen den Sinn von {kingdom} für Blut und Sippe.'),
 q('疑而却之', '疑而卻之', 'Doubt and refuse', 'Усомниться и отказать', '疑って拒む', 'Zweifeln und ablehnen'),
 q('疑其来历，婉拒入籍，以安朝局。', '疑其來歷，婉拒入籍，以安朝局。', 'Doubt his provenance, politely refuse, and steady the court.', 'Усомниться в происхождении, вежливо отказать и успокоить двор.', '素性を疑い、帰籍を婉曲に断り、朝局を安んじる。', 'Seine Herkunft bezweifeln, höflich ablehnen und den Hof beruhigen.'),
 q('来者拂袖而去，朝中惋惜与称快之声并起。', '來者拂袖而去，朝中惋惜與稱快之聲並起。', 'The newcomer leaves in a huff; voices of regret and of jeering rise together in court.', 'Пришелец уходит в негодовании; во дворе смешались сожаление и злорадство.', '来た者は袂を払って去る——朝中に惜しむ声と快哉の声が並び立つ。', 'Der Fremde geht verärgert davon; im Hof mischen sich Bedauern und Hohn.')
)
ev('king_brew', 2,
 ('御酿赐友', '御釀賜友', 'The Royal Brew', 'Королевский настой', '御醸を賜う', 'Der königliche Gebräu'),
 ('酒坊新成御酿百坛，礼官请以馈赠列国使节，为{king}广结友邦。', '酒坊新成御釀百壇，禮官請以饋贈列國使節，為{king}廣結友邦。', 'The wine house finishes a hundred jars of royal brew; the rite-master asks to gift them to envoys, winning {king} friends abroad.', 'Винный дом закончил сотню кувшинов королевского настоя; церемониймейстер просит одарить послов, чтобы {king} приобрёл друзей.', '酒坊が御醸百壇を仕立てる——礼官が列国使節に贈り、{king}の友邦を広げることを願う。', 'Das Weinhaus vollendet hundert Krüge königlichen Gebräus; der Zeremonienmeister bittet, sie den Gesandten zu schenken und {king} Freunde zu gewinnen.'),
 q('开坛馈赠', '開壇饋贈', 'Send it abroad', 'Одарить послов', '贈り届ける', 'Verschenken'),
 q('选百坛佳酿，分赠列国使节。', '選百壇佳釀，分贈列國使節。', 'Choose the hundred jars and send them to the envoys.', 'Отобрать кувшины и разослать послам.', '百壇の佳醸を選び、列国使節に分け贈る。', 'Die hundert Krüge aussuchen und den Gesandten schicken.'),
 q('御酿所至，列国尽欢，惟酒库与金库同减。', '御釀所至，列國盡歡，惟酒庫與金庫同減。', 'Where the brew arrives, foreign courts rejoice, only the wine vault and the gold vault shrink alike.', 'Куда ни прибудет настой — радость; лишь винный погреб и казна худеют разом.', '御醸の届く先々で列国が大喜び——ただ酒庫も金庫も同様に減る。', 'Wo das Gebräu hinkommt, jubeln fremde Höfe; nur Weinkeller und Schatzkammer schrumpfen gleichermaßen.'),
 q('留以自用', '留以自用', 'Keep it royal', 'Оставить себе', '宮に留める', 'Am Hof behalten'),
 q('留作内廷之用，不费分文。', '留作內廷之用，不費分文。', 'Keep it for the inner court; not a coin spent.', 'Оставить для дворца; ни монеты.', '内廷の用に留め、一文も費やさぬ。', 'Für den Hof behalten; kein Heller wird ausgegeben.'),
 q('御酿陈于酒库，别无动静，只待来年开坛。', '御釀陳於酒庫，別無動靜，只待來年開壇。', 'The royal brew rests in the cellar; nothing stirs, only the unsealing next year.', 'Королевский настой стоит в погребе; ничего не происходит, лишь вскрытие в будущем году.', '御醸は酒庫に貯えられ、動きなし——来年の開封を待つのみ。', 'Das königliche Gebräu ruht im Keller; nichts rührt sich, nur das Entsiegeln im nächsten Jahr.')
)
ev('heir_adoption', 2,
 ('宗室立嗣', '宗室立嗣', 'The Adopted Heir', 'Приёмный наследник', '宗室の嗣', 'Der adoptierte Erbe'),
 ('储位空缺，宗室少年被举为嗣，然其生父当年曾与{king}有隙。', '儲位空缺，宗室少年被舉為嗣，然其生父當年曾與{king}有隙。', 'The throne lacks an heir; a youth of the royal line is nominated, though his father once stood against {king}.', 'Трон без наследника; назван юноша царской крови, хотя его отец некогда вставал против {king}.', '儲位が空き、宗室の少年が跡継ぎに挙げられる——ただしその生父は昔{king}と隙があった。', 'Der Thron hat keinen Erben; ein Jüngling der königlichen Linie wird benannt, obgleich sein Vater einst gegen {king} stand.'),
 q('纳为嗣子', '納為嗣子', 'Adopt the boy', 'Удочерить', '嗣子に迎える', 'Den Jungen adoptieren'),
 q('不计前嫌，纳为储嗣，付以国本。', '不計前嫌，納為儲嗣，付以國本。', 'Lay the old grudge aside, make him heir, and entrust the realm\'s root to him.', 'Отбросить старую обиду, сделать его наследником и доверить корень державы.', '昔の遺恨を気にせず、儲嗣に迎え、国の本を託す。', 'Den alten Groll beiseitelegen, ihn zum Erben machen und ihm die Wurzel des Reiches anvertrauen.'),
 q('嗣子入宫，前嫌尽释，列国颂{king}以国为重。', '嗣子入宮，前嫌盡釋，列國頌{king}以國為重。', 'The heir enters the palace, the old grudge dissolves; foreign courts praise {king} for placing the realm first.', 'Наследник вошёл во дворец, обида растаяла; чужие дворы славят {king} — держава превыше всего.', '嗣子が宮に入り、遺恨が尽きる——列国が{king}の国を重んずるを讃える。', 'Der Erbe zieht ein, der Groll schmilzt; fremde Höfe preisen {king}, der das Reich voranstellt.'),
 q('另择宗亲', '另擇宗親', 'Choose another', 'Выбрать другого', '別の宗親を', 'Einen anderen wählen'),
 q('惧其后患，改择旁支宗亲。', '懼其後患，改擇旁支宗親。', 'Fear the consequences; pick a collateral branch instead.', 'Убояться последствий; выбрать боковую ветвь.', '後の患いを恐れ、傍系の宗親を改めて選ぶ。', 'Die Folgen fürchten; statt seiner eine Seitenlinie wählen.'),
 q('少年含恨出宫，宗室为此议论纷纷。', '少年含恨出宮，宗室為此議論紛紛。', 'The youth leaves with rancor; the royal kin debate it endlessly.', 'Юноша уходит с обидой; родня бесконечно обсуждает.', '少年が恨みを抱いて宮を出る——宗室はこのため喧々囂々。', 'Der Jüngling geht mit Groll; die Sippe bespricht es endlos.')
)
