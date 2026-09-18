namespace ET
{

public static class TimeHelper
{
    /// <summary>
    ///     一秒毫秒数量。
    /// </summary>
    public const int OneSecond = 1000;

    public static long Now()
    {
        return TimeInfo.Instance.ServerNow();
    }

    public static long ServerNow()
    {
        return TimeInfo.Instance.ServerNow();
    }

    public static long ClientNow()
    {
        return TimeInfo.Instance.ServerNow();
    }

    /// <summary>
    ///     配置文件时间转换器，用于Buff/Skill/Elemental等时间的转换<br />
    ///     【规定】：配置文件中时间如果是int,long类型，单位本身就是毫秒，如果是float或者double表示秒数<br />
    ///     （备注，ExcelExport有bug，float类型在某些数值上会报错)
    ///     配置文件中的时间单元最好使用毫秒数，但是为了方便，也支持毫秒，但是单位要按照上面的【约定】.
    /// </summary>
    /// <param name="ms"></param>
    /// <returns></returns>
    // 整数类型：直接返回
    public static long ToMS(int ms)
    {
        return ms;
    }

    /// <summary>
    ///     配置文件时间转换器，用于Buff/Skill/Elemental等时间的转换<br />
    ///     【规定】：配置文件中时间如果是int,long类型，单位本身就是毫秒，如果是float或者double表示秒数<br />
    ///     （备注，ExcelExport有bug，float类型在某些数值上会报错)
    ///     配置文件中的时间单元最好使用毫秒数，但是为了方便，也支持毫秒，但是单位要按照上面的【约定】.
    /// </summary>
    public static long ToMS(long ms)
    {
        return ms;
    }

    /// <summary>
    ///     配置文件时间转换器，用于Buff/Skill/Elemental等时间的转换<br />
    ///     【规定】：配置文件中时间如果是int,long类型，单位本身就是毫秒，如果是float或者double表示秒数<br />
    ///     （备注，ExcelExport有bug，float类型在某些数值上会报错)
    ///     配置文件中的时间单元最好使用毫秒数，但是为了方便，也支持毫秒，但是单位要按照上面的【约定】.
    /// </summary>
    public static long ToMS(float seconds)
    {
        return (long)(seconds * OneSecond);
    }

    /// <summary>
    ///     配置文件时间转换器，用于Buff/Skill/Elemental等时间的转换<br />
    ///     【规定】：配置文件中时间如果是int,long类型，单位本身就是毫秒，如果是float或者double表示秒数<br />
    ///     （备注，ExcelExport有bug，float类型在某些数值上会报错)
    ///     配置文件中的时间单元最好使用毫秒数，但是为了方便，也支持毫秒，但是单位要按照上面的【约定】.
    /// </summary>
    public static long ToMS(double seconds)
    {
        return (long)(seconds * OneSecond);
    }
    
    public static float MsToSec(long ms)
    {
        return ms / (float)OneSecond;
    }
}

}