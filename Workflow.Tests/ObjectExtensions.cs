namespace Workflow.Tests
{
    public static class ObjectExtensions
    {
        public static object Get(this object T, string propName)
        {
            return T.GetType().GetProperty(propName)?.GetValue(T, null);
        }
    }
}
