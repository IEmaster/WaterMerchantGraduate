from pathlib import Path
import json,csv
root=Path(__file__).resolve().parents[1]
rows=json.loads((root/'localization/strings.json').read_text(encoding='utf-8'))
template=(root/'tools/Localization.template.cs.txt').read_text(encoding='utf-8')
source=template.replace('ROWS','\n'.join('   { '+json.dumps(k)+', new[]{'+', '.join(json.dumps(v,ensure_ascii=False) for v in texts)+'} },' for k,texts in rows.items()))
(root/'src/WaterMerchantGraduateLocalization.cs').write_text(source,encoding='utf-8-sig')
for index,locale in enumerate(['zh','zh-Hant','en']):
 with (root/'package/Localization'/(locale+'.csv')).open('w',encoding='utf-8-sig',newline='') as f:
  writer=csv.writer(f)
  for key,texts in rows.items(): writer.writerow([key,texts[index].replace('\n','\\n')])
print('Generated three-language runtime and native CSV files.')
