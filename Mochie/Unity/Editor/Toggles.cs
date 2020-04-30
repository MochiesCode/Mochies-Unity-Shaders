using System.Collections.Generic;

public class Toggles {

	bool[] toggles; 
	string[] headers;
	List<int> mainFoldouts = new List<int>();
	List<int> subFoldouts = new List<int>();

	public Toggles(bool[] t, string[] h){
		toggles = t;
		headers = h;
		for (int i = 0; i < h.Length; i++){
			if (h[i].ToUpper() == h[i])
				mainFoldouts.Add(i);
			else
				subFoldouts.Add(i);
		}
	}

	public bool GetState(int index){return toggles[index];}
	public bool GetState(string index){
		bool state = false;
		for (int i = 0; i < headers.Length; i++){
			if (headers[i].ToLower() == index.ToLower()){
				state = toggles[i];
			}
		}
		return state;
	}

	public void SetState(int index, bool state){toggles[index] = state;}
	public void SetState(string index, bool state){
		for (int i = 0; i < headers.Length; i++){
			if (headers[i].ToLower() == index.ToLower()){
				 toggles[i] = state;
			}
		}
	}

	public int[] GetMain(){return mainFoldouts.ToArray();}
	public int[] GetSub(){return subFoldouts.ToArray();}
	public bool[] GetToggles(){return toggles;}
}