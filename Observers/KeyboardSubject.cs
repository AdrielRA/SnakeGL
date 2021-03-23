using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGL.Observers
{
    class KeyboardSubject : ISubject
    {
        public int key { get; set; }
        public bool special { get; set; }
        private List<IObserver> observers = new List<IObserver>();
               
        public void Attach(IObserver observer) => observers.Add(observer);
        public void Detach(IObserver observer) => observers.Remove(observer);
        public void Notify() => observers.ForEach(o => o.Update(this));        

        public void OnKeyboard(int key, bool special = false)
        {
            this.key = key;
            this.special = special;
            Notify();
        }
    }
}
