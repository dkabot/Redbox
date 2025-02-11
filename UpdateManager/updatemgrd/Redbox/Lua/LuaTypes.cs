namespace Redbox.Lua
{
    internal enum LuaTypes
    {
        None = -1, // 0xFFFFFFFF
        Nil = 0,
        Boolean = 1,
        LightUserData = 2,
        Number = 3,
        String = 4,
        Table = 5,
        Function = 6,
        UserData = 7,
    }
}
