using System.Collections.Generic;
using System.Linq;

namespace SeedTable {
    class SheetsConfig {
        public SheetsConfig(
            IEnumerable<string> only,
            IEnumerable<string> ignore,
            IEnumerable<string> subdivide = null,
            IEnumerable<string> mapping = null,
            IEnumerable<string> alias = null
        ) {
            var subdivideSheetNames = SheetNameWithSubdivides.FromMixed(subdivide);
            OnlySheetNames = SheetNameWithSubdivides.FromMixed(only);
            IgnoreSheetNames = SheetNameWithSubdivides.FromMixed(ignore);
            SubdivideRules = new SheetNameWithSubdivides(subdivideSheetNames.Concat(OnlySheetNames));
            excelToYamlMapping = mapping.Select(map => map.Split(':')).ToDictionary(map => map[1], map => map[0]);
            excelToYamlAlias = alias.Select(map => map.Split(':')).ToDictionary(map => map[1], map => map[0]);
        }

        SheetNameWithSubdivides SubdivideRules;
        SheetNameWithSubdivides IgnoreSheetNames;
        SheetNameWithSubdivides OnlySheetNames;
        Dictionary<string, string> excelToYamlMapping;
        Dictionary<string, string> excelToYamlAlias;

        // onOperationはFrom | Toだと正しく動作しない
        public bool IsUseSheet(string fileName, string sheetName, OnOperation onOperation) {
            if (IgnoreSheetNames.Contains(fileName, sheetName, onOperation)) return false;
            if (OnlySheetNames.Count != 0 && !OnlySheetNames.Contains(fileName, sheetName, onOperation)) return false;
            // エイリアス設定先のシートはfrom時変換されない
            if (onOperation.HasFlag(OnOperation.From) && excelToYamlAlias.ContainsKey(sheetName)) return false;
            return true;
        }

        public SheetNameWithSubdivide subdivide(string fileName, string sheetName, OnOperation onOperation) {
            var subdivideRule = SubdivideRules.Find(fileName, sheetName, onOperation);
            return subdivideRule ?? new SheetNameWithSubdivide(fileName, sheetName);
        }

        public string YamlTableName(string excelSheetName) {
            string yamlTableName;
            if (excelToYamlMapping.TryGetValue(excelSheetName, out yamlTableName)) {
                return yamlTableName;
            } else if (excelToYamlAlias.TryGetValue(excelSheetName, out yamlTableName)) {
                return yamlTableName;
            } else {
                return excelSheetName;
            }
        }
    }
}