using YamlDotNet.Serialization;

namespace Novex.Analyzer.V2.Engine;

/// <summary>
/// YAML 规则加载器
/// </summary>
public class YamlRuleLoader
{
    /// <summary>
    /// 从 YAML 字符串加载规则书
    /// </summary>
    public RuleBook LoadFromYaml(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
            throw new ArgumentException("YAML 内容不能为空", nameof(yaml));

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                .Build();

            var ruleBook = deserializer.Deserialize<RuleBook>(yaml);
            return ruleBook ?? new RuleBook();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"YAML 解析失败: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// 从文件加载规则书
    /// </summary>
    public RuleBook LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"规则文件不存在: {filePath}");
        
        var yaml = File.ReadAllText(filePath);
        return LoadFromYaml(yaml);
    }
    
    /// <summary>
    /// 将规则书保存为 YAML
    /// </summary>
    public string SaveToYaml(RuleBook ruleBook)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();
        return serializer.Serialize(ruleBook);
    }
    
    /// <summary>
    /// 将规则书保存到文件
    /// </summary>
    public void SaveToFile(RuleBook ruleBook, string filePath)
    {
        var yaml = SaveToYaml(ruleBook);
        File.WriteAllText(filePath, yaml);
    }
}

