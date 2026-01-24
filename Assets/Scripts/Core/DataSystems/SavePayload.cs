using System.Collections.Generic;

public enum SaveOperation
{
    INSERT,
    UPDATE,
    DELETE
}


public class SavePayload
{
    public SaveOperation Operation;

    public string Table;

    // INSERT / UPDATE 시 사용
    public Dictionary<string, object> Values = new();

    // WHERE 조건 (UPDATE / DELETE)
    public Dictionary<string, object> Conditions = new();
}