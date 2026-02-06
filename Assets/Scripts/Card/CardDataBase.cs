// 卡牌数据库（包含硬编码CSV解析）
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class CardDatabase
{
    [System.Serializable]
    public class CardData
    {
        public int id;
        public string name;
        public int manaCost;
        public string effect;
        public string imagePath;
        public string remark;
    }
    
    private static Dictionary<int, CardData> cardDataDict = new Dictionary<int, CardData>();
    
    // 硬编码的CSV相对路径
    private const string CSV_RELATIVE_PATH = "Resources/cards.csv";
    
    // 静态构造函数，自动加载数据
    static CardDatabase()
    {
        LoadCardData();
    }
    
    // 加载卡牌数据
    public static void LoadCardData()
    {
        try
        {
            // 构建完整路径（相对于Application.dataPath）
            string fullPath = Path.Combine(Application.dataPath, CSV_RELATIVE_PATH);
            
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"CSV文件不存在: {fullPath}");
                return;
            }
            
            string csvContent = File.ReadAllText(fullPath);
            ParseCsvContent(csvContent);
        }
        catch (Exception e)
        {
            Debug.LogError($"加载卡牌数据失败: {e.Message}");
        }
    }
    
    // 解析CSV内容
    private static void ParseCsvContent(string csvContent)
    {
        cardDataDict.Clear();
        
        using (StringReader reader = new StringReader(csvContent))
        {
            string line;
            bool isFirstLine = true;
            
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                if (isFirstLine)
                {
                    isFirstLine = false; // 跳过标题行
                    continue;
                }
                
                // 解析CSV行
                string[] fields = ParseCsvLine(line);
                if (fields.Length < 6) continue;
                
                // 创建卡牌数据对象
                CardData data = new CardData
                {
                    id = int.Parse(fields[0]),
                    name = fields[1],
                    manaCost = int.Parse(fields[2]),
                    effect = fields[3],
                    imagePath = fields[4],
                    remark = fields[5]
                };
                
                // 添加到字典
                cardDataDict[data.id] = data;
            }
        }
        
        Debug.Log($"成功加载 {cardDataDict.Count} 张卡牌数据");
    }
    
    // 解析CSV行（简单实现）
    private static string[] ParseCsvLine(string line)
    {
        // 简单分割（适用于没有逗号的字段）
        return line.Split(',');
        
        /* 如果需要处理引号内的逗号，可以使用更复杂的解析：
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        fields.Add(currentField);
        return fields.ToArray();
        */
    }
    
    // 获取卡牌数据
    public static CardData GetCardData(int id)
    {
        if (cardDataDict.TryGetValue(id, out CardData data))
        {
            return data;
        }
        
        Debug.LogError($"未找到卡牌数据: ID={id}");
        return null;
    }
}
