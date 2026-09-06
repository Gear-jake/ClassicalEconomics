# -*- coding: utf-8 -*-
"""v1.6.0 de Events (30 Events). Idempotent."""
import io, json, collections

path = 'Locales/de.json'
existing = json.load(io.open(path, encoding='utf-8'), object_pairs_hook=collections.OrderedDict)
en = json.load(io.open('Locales/en.json', encoding='utf-8'))

DE = collections.OrderedDict()
def ev(eid, n, title, desc, *opts):
    DE['ev_' + eid] = title; DE['ev_' + eid + '_desc'] = desc
    for i in range(n):
        a, b, c = opts[i]
        DE['ev_%s_opt%d' % (eid, i+1)] = a
        DE['ev_%s_opt%d_desc' % (eid, i+1)] = b
        DE['ev_%s_res%d' % (eid, i+1)] = c
def q(a, b, c): return (a, b, c)

# finance
ev('treasury_gap', 2, 'Leere Schatzkammer', 'Die Schatzkammer ist fast leer; die Minister streiten über leere Bücher.',
    q('Notsteuer erheben', 'Eine Notsteuer von allen Einwohnern erheben', 'Die Notsteuer wurde erhoben; die Schatzkammer atmet auf.'),
    q('Durchhalten', 'Nichts tun; der Groll wächst', 'Die Kammer bleibt leer; der Groll wächst.'))
ev('tax_corruption', 2, 'Korrupte Steuereintreiber', 'Die Eintreiber unterschlagen; weniger Gold erreicht die Kammer.',
    q('Bestrafen', 'Die Korrupten werden purge; das Ansehen der Krone steigt', 'Die Korrupten wurden ge Säubert; das Ansehen stieg.'),
    q('Sich teilen', 'Die Krone teilt die Beute; Unzufriedenheit gärt', 'Krone und Diebe teilen; Unmut gärt.'))
ev('rich_petition', 2, 'Die Petition der Reichen', 'Reiche Kaufleute bieten Gold für Handelsprivilegien.',
    q('Gewähren', 'Das Gold nehmen und die Rechte gewähren', 'Gold fließt; die Kaufleute feiern.'),
    q('Ablehnen', 'Die Petition ablehnen und Strafen verhängen', 'Abgelehnt; die Kaufleute sind entfremdet.'))
ev('merchant_loan', 2, 'Der Fremdkredit', 'Die Kaufmannsliga bietet ein Vermögen als Darlehen an — retour in zwei Jahren.',
    q('Vertrag schließen', 'Das Gold nehmen; in zwei Jahren mit Zinsen zurückzahlen', 'Das Gold ist da; {king} versiegelte den Vertrag.'),
    q('Ablehnen', 'Keine Fremdschulden; {kingdom} verwaltet seine eigene Tasche', 'Angebot abgelehnt; keine Fremdschulden.'))

# disaster
ev('drought', 2, 'Große Dürre', 'Tage des Regens fehlen; das Land Risse zeigt und die Preise klettern.',
    q('Kornkammern öffnen', 'Getreide an das Volk verteilen — kostet die Kammer', 'Das Korn wurde verteilt; das Volk überlebte.'),
    q('Für Regen beten', 'Altäre gebaut und abgerissen; kein Regen, nur Wut', 'Die Altäre wurden abgebaut; Wut blieb.'))
ev('locust', 2, 'Heuschrecken', 'Ein Schwarm verfinstert den Himmel; die Felder werden kahl.',
    q('Prämie zahlen', 'Haushalte fangen Heuschrecken für Getreide', 'Jeder Haushalt jagt; die Plage ist eingedämmt.'),
    q('dem Schicksal überlassen', 'Der Schwarm hinterlässt nichts; Hunger in den Dörfern', 'Der Schwarm ging; Hunger klingt in den Dörfern.'))
ev('quake_aftermath', 2, 'Nach dem Beben', 'Nachbeben halten; Gerüchte über das Verderben kursieren.',
    q('Arbeitsfürsorge', 'Bezahlte Reparaturen bauen Häuser und beruhigen', 'Reparaturen beruhigten das Volk.'),
    q('ignorieren', 'Die Ruinen bleiben; Gerüchte wachsen', 'Die Ruinen bleiben; Gerüchte wuchern.'))
ev('flood', 2, 'Die Flut', 'Tage des Regens; der Fluss verschluckt die Dörfer.',
    q('Deiche reparieren', 'Alle Hände auf die Deiche', 'Die Deiche halten; {kingdom} behält seine Heimat.'),
    q('gewähren lassen', 'Vertriebene zerstreuen sich; Groll in jeder Stadt', 'Die Flut hinterlässt Trümmer und Groll.'))
ev('wildfire', 2, 'Der Waldbrand', 'Trockener Wind treibt das Feuer zu Dörfern und Holz.',
    q('Schneisen schlagen', 'Holzfäller bezahlen, um Schneisen zu schlagen', 'Die Schneisen stoppen die Flammen; der Wald bleibt.'),
    q('brennen lassen', 'Nichts ausgeben; das Feuer brennt aus', 'Aschefelder und weinende Dörfer.'))
ev('blizzard', 2, 'Der Blizzard', 'Der Blizzard tötet Herden und blockiert die Getreiderouten.',
    q('Kornkammern öffnen', 'Getreide leihen, im Frühling zurückzahlen', 'Getreide geliehen; der Winter ist überstanden.'),
    q('zuhause bleiben', 'Getreide sparen; zähle die Gefrorenen', 'Als der Schnee aufhörte, trauerte jedes Dorf.'))

# court
ev('royal_wedding', 2, 'Eine Königliche Hochzeit', 'Eine königliche Hochzeit naht; der Kämmerer bringt zwei Haushaltsentwürfe.',
    q('Großes Fest', 'Ein Fest, das jeder Hof sich merken wird', 'Das Fest beeindruckt; Gesandte füllen die Tore.'),
    q('Bescheiden', 'Ein schlichter Ritus lässt die Höfe unbeeindruckt', 'Der schlichte Ritus hinterlässt keinen Eindruck.'))
ev('minister_power', 2, 'Der Allmächtige Minister', 'Ein Minister verpackt die Höfe; Erlasse tragen nur sein Siegel.',
    q('Ihn brechen', 'Gegen den Minister vorgehen; seine Fraktion gärt', 'Die Macht kehrt zur Krone zurück; die Fraktion kocht.'),
    q('Tolerieren', 'Die Dinge ruhen lassen, um den Hof nicht zu erschüttern', 'Der Minister herrscht weiter; alle Höfe wissen es.'))
ev('succession_dispute', 2, 'Der Thronfolgestreit', 'Die Nachfolge ist umstritten; zwei Fraktionen liegen im Streit.',
    q('Den Älteren unterstützen', 'Der Tradition folgen; der ältere Erbe siegt', 'Der Ältere siegt; die Verlierer grübeln.'),
    q('Den Würdigeren wählen', 'Den Besseren wählen und die anderen abfinden', 'Gold erreicht den Streit — auf Kosten der Kammer.'))

# military
ev('army_pay', 2, 'Die Armee fordert Lohn', 'Der Grenzarmee fehlt monatelang Lohn; Gesandte fordern Gold.',
    q('Voll bezahlen', 'Die Armee vollständig bezahlen; die Moral steigt', 'Die Armee wurde bezahlt; die Moral steigt.'),
    q('Verschieben', 'Die Zahlung verschieben; Groll im Lager', 'Die Zahlung wurde verschoben; Groll im Lager.'))
ev('mercenary_default', 2, 'Söldner fordern Lohn', 'Gelbe Söldner fordern ausstehenden Lohn im ungünstigsten Moment.',
    q('Den Schulden nachkommen', 'Die Schuld begleichen; die Söldner beruhigen sich', 'Die Schuld beglichen; die Söldner sind ruhig.'),
    q('Auflösen', 'Sie auflösen; die Söldner werden zu Räubern', 'Aufgelöst; die Söldner werden zu Räubern.'))
ev('prisoner_ransom', 2, 'Das Lösegeld', 'Der Feind bietet Lösegeld für gefangene Adlige an.',
    q('Lösegeld zahlen', 'Das Lösegeld zahlen und unsere Leute heimholen', 'Die Gefangenen kommen heim; die Spannung löst sich.'),
    q('Ablehnen', 'Ablehnen — ein eisernes Gesicht zeigen', 'Abgelehnt; der Feind schwört Rache.'))

# civil
ev('bread_riot', 2, 'Der Brotaufruhr', 'Die Brotpreise schießen hoch; eine Menge versammelt sich an den Kornkammern.',
    q('Kornkammern öffnen', 'Getreide zu fairen Preisen verkaufen', 'Getreide zu fairen Preisen; die Straßen beruhigen sich.'),
    q('Zerstäuben', 'Die Menge gewaltsam zerstreuen', 'Die Menge ist zerstreut; die Wut nicht.'))
ev('plague', 2, 'Die Pest', 'Die Pest breitet sich in der Nachbarprovinz aus; die Ärzte bitten zu handeln.',
    q('Ärzte finanzieren', 'Ärzte und Medizin bezahlen; die Pest bleibt draußen', 'Ärzte bezahlt; die Pest bleibt draußen.'),
    q('Tore schließen', 'Die Tore schließen und abwarten', 'Die Tore sind zu; der Handel stirbt.'))

# diplomacy
ev('neighbor_extort', 2, 'Das Ultimatum', 'Ein starker Nachbar verlangt Tribut — zahle, oder wir patrouillieren.',
    q('Den Tribut zahlen', 'Den Tribut zahlen und Frieden kaufen', 'Der Tribut wurde gezahlt; der Nachbar verstummt — vorerst.'),
    q('Kategorisch ablehnen', 'Kategorisch ablehnen — Krieg, wenn es sein muss', 'Der Gesandte wurde vertrieben; die Grenze ist angespannt.'))
ev('marriage_alliance', 2, 'Der Heiratsantrag', 'Ein Nachbar schlägt eine Heiratsallianz vor.',
    q('Annehmen', 'Die Ehe verbindet die beiden Höfe', 'Die Ehe verbindet; Gesandte verkehren ununterbrochen.'),
    q('Höflich ablehnen', 'Höflich ablehnen und die Unabhängigkeit bewahren', 'Höflich abgelehnt.'))

# bank/commerce
ev('merchant_loan', 2, 'Der Fremdkredit', 'Die Kaufmannsliga bietet ein Vermögen — retour in zwei Jahren.',
    q('Vertrag schließen', 'Das Gold nehmen; mit Zinsen zurückzahlen', 'Das Gold ist da; der Vertrag ist versiegelt.'),
    q('Ablehnen', 'Keine Fremdschulden; {kingdom} führt seine eigene Kasse', 'Abgelehnt; keine Fremdschulden.'))
ev('loan_due', 2, 'Die Fälligkeit', 'Zwei Jahre sind um; die Sammler stehen vor den Toren.',
    q('Voll zurückzahlen', 'Die Kammer bluten lassen, aber den Kredit bewahren', 'Der Kredit ist beglichen; das Ansehen wuchs.'),
    q('Nicht zahlen', 'Den Vertrag zerreiben; die Verachtung jedes Hofs ernten', 'Der Vertrag zerrissen; die Kaufleute erinnern sich.'))
ev('guild_petition2', 2, 'Die Gildenbank', 'Die Handwerksgilden wollen eine Bank und bieten der Krone einen Anteil.',
    q('Investieren', 'Investieren und Dividenden kassieren — in einem Boot mit den Gilden', 'Die Bank eröffnet; die Krone kassiert jährlich.'),
    q('Ablehnen', 'Den Namen der Krone aus dem Geldverleih heraushalten', 'Abgelehnt; stattdessen Armenfürsorge.'))
ev('bank_run', 2, 'Der Bank Run', 'Ausfälle verbreiten sich; Einleger stürmen die Banken. {king} muss jetzt handeln.',
    q('Kronen-Nothilfe', 'Die Reserven aus der Kronenkammer füllen', 'Die Nothilfe beruhigt den Run; der Markt atmet auf.'),
    q('Zusammenbruch zulassen', 'Die Vault retten, nicht die Banken — der Zorn wächst wie eine Lawine', 'Bankpleiten； die Straßen voller wütender Einleger.'))
ev('bank_run_aftermath', 2, 'Nach dem Run', 'Der Run ist vorbei, aber das Vertrauen ist zerbrochen. Wie wird {king} es flicken?',
    q('Hilfe für die Ruinierten', 'Gold für die ruinierten Einleger ausgeben', 'Die Geretteten gewinnen Vertrauen; der Markt wärmt sich auf.'),
    q('Den Markt heilen lassen', 'Nicht eingreifen; die Zeit heilt alle Bücher', 'Die Heilung ist langsam; die Narbe bleibt.'))
ev('caravan_ambush', 2, 'Der Karawanenüberfall', 'Die Karawane von {king} wurde überfallen; die Handelsstraßen haben Angst.',
    q('Eskorten bezahlen', 'Eskorten anheuern und die Straßen sichern', 'Eskorten reiten aus; die Karawane bewegt sich wieder.'),
    q('Selbst überlassen', 'Keine Garantien; die Straßen leiden zwei Jahre', 'Die Straßen sind abgeschnitten; die Schuld liegt bei {kingdom}.'))
ev('trade_route_cut', 2, 'Die Blockierten Routen', 'Ein halbes Jahr blockierte Routen; die Läden sind leer, die Steuern schrumpfen.',
    q('Wiederaufbau finanzieren', 'Die richtigen Handflächen fetten; die Routen öffnen sich wieder', 'Die Routen öffnen sich wieder; die Handelssteuer erholt sich.'),
    q('Landweg', 'Der Landumweg ist langsam und teuer — aber niemand wird bestochen', 'Der Landweg hält kaum; der Handel bleibt über null.'))

json_data = collections.OrderedDict()
json_data.update(DE)
merged = 0
for k, v in DE.items():
    if existing.get(k) == en.get(k):
        existing[k] = v; merged += 1
json.dump(existing, io.open(path, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
io.open(path, 'a', encoding='utf-8').write('\n')
print('DE events merged:', merged, 'total:', len(existing))

t2 = sum(1 for k in en if existing.get(k) != en.get(k))
print('DE translated:', t2, '/', len(en), '(', round(t2/len(en)*100), '%)')
miss = [k for k in en if existing.get(k) == en.get(k)]
print('remaining:', len(miss))
