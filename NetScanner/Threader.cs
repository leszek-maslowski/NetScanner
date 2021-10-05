using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetScanner
{
    public class Threader
    {
        int currentSlot;
        Slot[] slots;
        bool breakState = false;

        public Threader(int threadCount)
        {
            currentSlot = -1;
            slots = new Slot[threadCount];
            for (int i = 0; i < threadCount; i++)
                slots[i] = new Slot(this);
        }

        public int Count()
        {
            return slots.Where(x => x != null).Sum(x => x.Count());
        }

        public bool Busy()
        {
            return slots.Any(x => !x.IsEmpty());
        }

        public void Enqueue(Action a)
        {
            if (slots != null)
            {
                currentSlot = (currentSlot + 1) % slots.Length;
                slots[currentSlot].Enqueue(a);
            }

        }

        public void Join()
        {
            while (slots.Any(x => !x.IsEmpty()))
                Thread.Sleep(100);
        }

        void Destroy()
        {
            breakState = true;
            slots = null;
        }

        class Slot
        {
            Threader parent;
            Thread th;
            Queue<Action> actions;
            object syncRoot;

            public bool IsEmpty()
            {
                return Count() == 0;
            }

            public int Count()
            {
                lock (syncRoot)
                    return actions.Count;
            }

            public Slot(Threader owner)
            {
                parent = owner;
                actions = new Queue<Action>();
                syncRoot = new object();

                th = new Thread(Spin);
                th.IsBackground = true;
                th.Start();
            }
            void Spin()
            {
                while (!parent.breakState)
                {
                    if (IsEmpty())
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    Action a;

                    lock (syncRoot)
                        a = actions.Peek();

                    a.Invoke();

                    lock (syncRoot)
                        actions.Dequeue();
                }
            }
            public void Enqueue(Action a)
            {
                lock (syncRoot)
                    actions.Enqueue(a);
            }
        }
    }


}
