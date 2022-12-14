namespace Manage
{
    public class BaseSingleton<T> where T : class, new()
    {
        private static T instance = null;

        public static T Instance
        {
            get
            {
                instance ??= new T();
                return instance;
            }
        }

        protected BaseSingleton() { }
    }
}
