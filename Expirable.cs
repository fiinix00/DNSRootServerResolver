using System;

namespace DNSRootServerResolver
{
    public class Expirable<T>
    {
        private bool initialized;

        private Func<T> renewer;
        private Func<T, DateTime> reexpirator;

        public DateTime ExpiresAt { get; set; }

        private T value;
        public T Value
        {
            get
            {
                Refresh();

                return value;
            }
            set { this.value = value; }
        }

        public Expirable(Func<T> renewer, Func<T, DateTime> reexpirator)
        {
            this.renewer = renewer;
            this.reexpirator = reexpirator;
        }

        public bool Expired 
        { 
            get 
            {
                if (!initialized) Refresh();

                return DateTime.Now > ExpiresAt; 
            } 
        }

        public void Refresh()
        {
            if (!initialized || Expired)
            {
                this.value = this.renewer();
                this.ExpiresAt = this.reexpirator(this.value);
            }

            initialized = true;
        }
    }
}
