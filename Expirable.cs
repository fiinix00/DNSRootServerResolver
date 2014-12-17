using System;

namespace DNSRootServerResolver
{
    public class Expirable<T>
    {
        private Func<T> renewer;
        private Func<T, DateTime> reexpirator;

        public DateTime ExpiresAt { get; set; }

        public T Value { get; set; }

        public Expirable(Func<T> renewer, Func<T, DateTime> reexpirator)
        {
            this.renewer = renewer;
            this.reexpirator = reexpirator;

            this.Refresh();
        }

        public bool Expired { get { return DateTime.Now > ExpiresAt; } }

        public void Refresh()
        {
            Value = this.renewer();
            ExpiresAt = this.reexpirator(Value);
        }

        public T RefreshIfExpired()
        {
            if (Expired)
            {
                Refresh();
            }

            return Value;
        }
    }
}
