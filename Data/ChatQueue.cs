using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CloudTalk.Data {
	public class ChatQueue<T> : List<T> {
		public void Enqueue(T item) {
			this.Add(item);
		}

		public T Dequeue() {
			var firstItem = this.FirstOrDefault();

			if (firstItem != null) {
				this.RemoveAt(0);
			}

			return firstItem;
		}
	}
}
