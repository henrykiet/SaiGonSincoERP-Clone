using reportSystem01.Shared;

namespace reportSystem01.Server.Helpers
{
    public class MenuDtoComparer : IEqualityComparer<MenuDto>
    {
        public bool Equals(MenuDto x, MenuDto y)
        {
            return x.MenuId == y.MenuId;
        }

        public int GetHashCode(MenuDto obj)
        {
            return obj.MenuId.GetHashCode();
        }
    }
}
