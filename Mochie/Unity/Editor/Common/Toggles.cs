using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Mochie {
    public class Toggles {

        bool[] toggles; 
        string[] headers;
        AnimBool[] fades;
        float[] layoutFades;
        EditorWindow animWindow;
        List<int> foldouts = new List<int>();
        
        public Toggles(string[] s, int defaultOffset){
            headers = s;
            toggles = new bool[s.Length];
            fades = new AnimBool[s.Length];
            layoutFades = new float[s.Length];

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

        public int IndexOf(string header){
            for (int i = 0; i < headers.Length; i++){
                if (string.Equals(headers[i], header, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        public bool GetState(string index){
            int i = IndexOf(index);
            return i >= 0 && toggles[i];
        }

        // Eased 0-1 open amount of a foldout, sliding toward its current state
        public float GetFade(int index){
            AnimBool fade = fades[index];
            if (fade == null){
                fade = new AnimBool(toggles[index]);
                fade.speed = Foldouts.animSpeed;
                fade.valueChanged.AddListener(RepaintWindow);
                fades[index] = fade;
                layoutFades[index] = fade.faded;
            }
            if (fade.target != toggles[index]){
                // States only change from clicks in the inspector, so that's the window to repaint while it slides
                animWindow = EditorWindow.focusedWindow;
                fade.target = toggles[index];
            }
            // The fade changes the layout, so it can only be sampled on Layout. Reading it live lets the
            // events after Layout build different groups than Layout did, and controls jump for a frame.
            if (Event.current.type == EventType.Layout)
                layoutFades[index] = fade.faded;
            return layoutFades[index];
        }

        // OnGUI can also run outside a window's own repaint (UI Toolkit tooltips), so repaint the window directly
        // from the animation's update instead of calling HandleUtility.Repaint() during OnGUI
        void RepaintWindow(){
            if (animWindow != null)
                animWindow.Repaint();
        }

        public void SetState(int index, bool state){ 
            toggles[index] = state; 
        }

        public void SetState(string index, bool state){
            int i = IndexOf(index);
            if (i >= 0)
                toggles[i] = state;
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