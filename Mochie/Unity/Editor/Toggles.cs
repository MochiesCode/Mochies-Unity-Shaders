using System.Collections.Generic;

namespace Mochie {
	public class Toggles {

		bool[] toggles; 
		string[] headers;
		List<int> mainFoldouts = new List<int>();
		List<int> subFoldouts = new List<int>();

		public Toggles(string[] s){
			headers = s;
			toggles = new bool[s.Length];
			toggles[0] = true;
			
			for (int i = 0; i < s.Length; i++){
				if (s[i].ToUpper() == s[i])
					mainFoldouts.Add(i);
				else
					subFoldouts.Add(i);
				if (i > 0)
					toggles[i] = false;
			}
		}

		public bool GetState(int index){ 
			return toggles[index]; 
		}

		public bool GetState(string index){
			bool state = false;
			for (int i = 0; i < headers.Length; i++){
				if (headers[i].ToLower() == index.ToLower()){
					state = toggles[i];
				}
			}
			return state;
		}

		public void SetState(int index, bool state){ 
			toggles[index] = state; 
		}

		public void SetState(string index, bool state){
			for (int i = 0; i < headers.Length; i++){
				if (headers[i].ToLower() == index.ToLower()){
					toggles[i] = state;
				}
			}
		}

		public bool[] GetToggles(){ 
			return toggles; 
		}
	}
}