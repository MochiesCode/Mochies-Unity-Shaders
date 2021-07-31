using System.Collections.Generic;
using UnityEngine;

namespace Mochie {
	public class Toggles {

		bool[] toggles; 
		string[] headers;
		List<int> foldouts = new List<int>();
		
		public Toggles(string[] s, int defaultOffset){
			headers = s;
			toggles = new bool[s.Length];

			for (int i = 0; i < s.Length; i++){
				if (i <= defaultOffset)
					toggles[i] = true;
			}
			
			for (int i = 0; i < s.Length; i++){
					foldouts.Add(i);
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

		public static void CollapseFoldouts(Material mat, Dictionary<Material, Toggles> foldouts, int startIndex){
			for (int i = startIndex; i <= foldouts[mat].GetToggles().Length-1; i++)
				foldouts[mat].SetState(i, false);
		}
	}
}