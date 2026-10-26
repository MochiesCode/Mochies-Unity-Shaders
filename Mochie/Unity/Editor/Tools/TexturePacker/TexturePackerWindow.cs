using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace Mochie {
    public class TexturePackerWindow : EditorWindow {
        const float OutputWidth = 300f;
        const float WideLayoutWidth = 700f;
        const float FloatingWidth = 880f;
        static readonly Vector2 DockedMinSize = new Vector2(400f, 500f);
        static readonly Vector2 DockedMaxSize = new Vector2(4000f, 4000f);
        const float ThumbnailSize = 64f;
        const int MaxSize = 16384;
        const string WikiURL = "https://github.com/MochiesCode/Mochies-Unity-Shaders/wiki/Utilities#texture-packer";

        static readonly string[] ChannelLetters = {"R", "G", "B", "A"};
        static readonly Color[] ChannelColors = {
            new Color(0.85f, 0.26f, 0.26f),
            new Color(0.27f, 0.68f, 0.31f),
            new Color(0.27f, 0.47f, 0.90f),
            new Color(0.55f, 0.55f, 0.55f)
        };
        static readonly Color HeaderColor = new Color(1f, 1f, 1f, 0.1f);
        static readonly Color PreviewBackground = new Color(0.12f, 0.12f, 0.12f);
        static readonly int[] Sizes = {32, 64, 128, 256, 512, 1024, 2048, 4096, 8192};
        static readonly string[] SizeLabels = new[]{"Auto (Largest Source)"}.Concat(Sizes.Select(s => s.ToString())).Concat(new[]{"Custom"}).ToArray();
        static readonly string[] ViewLabels = {"RGB", "R", "G", "B", "A"};
        static readonly int CustomSizeIndex = Sizes.Length + 1;

        static readonly GUIContent presetTip = new GUIContent("", "Sets up the channels for a packing format. Textures you've already assigned move to the channel with the same name.");
        static readonly GUIContent fillFromSelectionLabel = new GUIContent("Fill From Selection", "Assigns the textures selected in the Project window to the channels they're named after, like Rock_AO to Occlusion. Selecting a folder includes the textures inside it.");
        static readonly GUIContent resetLabel = new GUIContent("Reset", "Clears every channel back to the current preset.");
        static readonly GUIContent includeLabel = new GUIContent("Include", "Gives the packed texture an alpha channel. Without one it uses half the memory.");
        static readonly GUIContent nameTip = new GUIContent("", "What this channel holds. Presets and Fill From Selection match textures and channels by this name.");
        static readonly GUIContent textureLabel = new GUIContent("Texture", "The texture this channel is read from. Any channel of any 2D texture can be used.");
        static readonly GUIContent sourceLabel = new GUIContent("Channel", "Which channel of the texture to read. Grayscale combines red, green, and blue by their brightness.");
        static readonly GUIContent invertLabel = new GUIContent("Invert", "Flips the values, like turning a smoothness map into roughness.");
        static readonly GUIContent fillLabel = new GUIContent("Fill", "The value this channel is filled with, since it has no texture.");
        static readonly GUIContent thumbnailTip = new GUIContent("", "Click to show this channel in the preview.");
        static readonly GUIContent filteringLabel = new GUIContent("Filtering", "Adjusts the values read from the texture, from top to bottom, after Invert.");
        static readonly GUIContent resetFilteringLabel = new GUIContent("Reset", "Resets the filtering.");
        static readonly GUIContent brightnessLabel = new GUIContent("Brightness", "Multiplies the values.");
        static readonly GUIContent contrastLabel = new GUIContent("Contrast", "Pushes the values away from mid gray, or pulls them toward it below 1.");
        static readonly GUIContent gammaLabel = new GUIContent("Gamma", "Brightens the midtones above 1 and darkens them below 1, without changing black and white.");
        static readonly GUIContent inputLevelsLabel = new GUIContent("Input Levels", "The values that become black and white. Narrowing the range raises contrast, like Levels in an image editor.");
        static readonly GUIContent outputLevelsLabel = new GUIContent("Output Levels", "The range the final values are remapped to.");
        static readonly GUIContent samplingLabel = new GUIContent("Sampling", "How the texture is resized when it doesn't match the output size. Bilinear blends neighboring pixels, Point keeps hard pixel edges.");
        static readonly GUIContent sizeLabel = new GUIContent("Size", "The packed texture's resolution. Auto uses the largest source texture's full resolution, ignoring its Max Size.");
        static readonly GUIContent formatLabel = new GUIContent("Format", "The image format to save. Both store 8 bits per channel.");
        static readonly GUIContent sRGBLabel = new GUIContent("sRGB (Color Texture)", "Imports the packed texture as color. Turn it on when the channels hold color, like an albedo map. Leave it off for data such as occlusion, roughness, and metallic.");
        static readonly GUIContent outputLabel = new GUIContent("Output", "The texture Pack overwrites. Assign an existing PNG or TGA to pack into it, or leave it empty to choose a new file.");
        static readonly GUIContent packAsLabel = new GUIContent("Pack As...", "Packs into a new file.");

        static readonly List<TexturePackerWindow> openWindows = new List<TexturePackerWindow>();
        static PackerChannel clipboard;

        [SerializeField] PackerChannel[] channels;
        [SerializeField] bool includeAlpha;
        [SerializeField] bool sRGB;
        [SerializeField] string presetName = "";
        [SerializeField] string suffix = "";
        [SerializeField] int sizeIndex;
        [SerializeField] int customWidth = 1024;
        [SerializeField] int customHeight = 1024;
        [SerializeField] PackerFormat format;
        [SerializeField] Texture2D output;
        [SerializeField] int view;
        [SerializeField] bool[] showFiltering = new bool[4];
        [SerializeField] Vector2 channelScroll;
        [SerializeField] Vector2 outputScroll;

        PackerRenderer packer;
        Dictionary<Texture, Vector2Int> sourceSizes = new Dictionary<Texture, Vector2Int>();
        bool refreshQueued;
        bool resizeQueued;
        bool namingPreset;
        bool focusPresetName;
        string newPresetName = "";
        GUIContent menuIcon;
        GUIContent helpIcon;
        GUIStyle nameStyle;
        GUIStyle badgeStyle;
        GUIStyle paddingStyle;

        [MenuItem("Tools/Mochie/Texture Packer", priority = 1001)]
        static void Init(){
            TexturePackerWindow window = GetWindow<TexturePackerWindow>();
            window.titleContent = new GUIContent("Texture Packer");
            window.Show();
            if (!window.docked)
                window.position = new Rect(window.position.position, new Vector2(FloatingWidth, Mathf.Max(window.position.height, 500f)));
        }

        void OnEnable(){
            if (channels == null || channels.Length != 4 || channels.Any(c => c == null))
                ApplyPreset(PackerPresets.BuiltIn[0], false, false);
            if (showFiltering == null || showFiltering.Length != 4)
                showFiltering = new bool[4];
            packer = new PackerRenderer();
            openWindows.Add(this);
            Undo.undoRedoPerformed += OnUndoRedo;
            // A refresh queued before a script reload never runs, so don't let it block the next one
            refreshQueued = false;
            resizeQueued = false;
            QueueRefresh();
        }

        void OnDisable(){
            openWindows.Remove(this);
            Undo.undoRedoPerformed -= OnUndoRedo;
            if (packer != null)
                packer.Dispose();
            packer = null;
        }

        void OnUndoRedo(){
            sourceSizes.Clear();
            QueueRefresh();
        }

        // Called after assets are imported, so previews follow edits to the source files
        internal static void OnAssetsImported(string[] paths){
            if (openWindows.Count == 0)
                return;
            HashSet<string> imported = new HashSet<string>(paths);
            foreach (TexturePackerWindow window in openWindows){
                if (window.channels != null && window.channels.Any(c => c != null && c.texture != null && imported.Contains(AssetDatabase.GetAssetPath(c.texture)))){
                    window.sourceSizes.Clear();
                    window.QueueRefresh();
                }
            }
        }

        // Previews render outside of OnGUI, since loading a source can import assets
        void QueueRefresh(){
            if (refreshQueued)
                return;
            refreshQueued = true;
            EditorApplication.delayCall += Refresh;
        }

        void Refresh(){
            refreshQueued = false;
            if (this == null || packer == null)
                return;
            try {
                packer.RenderPreview(channels, includeAlpha, GetOutputSize(), GetShownView());
            }
            catch (Exception e){
                Debug.LogException(e);
            }
            Repaint();
        }

        void InitStyles(){
            if (nameStyle != null)
                return;
            nameStyle = new GUIStyle(EditorStyles.textField){ fontStyle = FontStyle.Bold };
            badgeStyle = new GUIStyle(EditorStyles.boldLabel){ alignment = TextAnchor.MiddleCenter, padding = new RectOffset() };
            badgeStyle.normal.textColor = Color.white;
            paddingStyle = new GUIStyle(){ padding = new RectOffset(6, 6, 4, 4) };
            menuIcon = EditorGUIUtility.IconContent("_Menu");
            menuIcon.tooltip = "Channel options";
            helpIcon = EditorGUIUtility.IconContent("_Help");
            helpIcon.tooltip = "Open the Texture Packer documentation";
        }

        void OnGUI(){
            InitStyles();
            DrawToolbar();
            if (namingPreset)
                DrawPresetNameBar();
            if (packer == null || !packer.IsValid){
                EditorGUILayout.HelpBox("The shader Hidden/Mochie/TexturePackerWindow is missing. Reimport Mochie's Shaders to restore it.", MessageType.Error);
                return;
            }

            // Floating windows always use the wide layout, since FitToContent locks their width
            if (!docked || position.width >= WideLayoutWidth){
                Rect body = EditorGUILayout.BeginHorizontal();
                channelScroll = EditorGUILayout.BeginScrollView(channelScroll);
                Rect channelsRect = EditorGUILayout.BeginVertical(paddingStyle);
                DrawChannels();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.BeginVertical(GUILayout.Width(OutputWidth));
                outputScroll = EditorGUILayout.BeginScrollView(outputScroll);
                Rect outputRect = EditorGUILayout.BeginVertical(paddingStyle);
                DrawOutput(OutputWidth - 24f);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                // Content rects only have their real sizes during Repaint. The body fills the visible area, so the
                // difference to the content is how much taller the window needs to be, whatever its tab and border take.
                if (Event.current.type == EventType.Repaint)
                    FitToContent(position.height + Mathf.Max(channelsRect.height, outputRect.height) - body.height);
            }
            else {
                channelScroll = EditorGUILayout.BeginScrollView(channelScroll);
                EditorGUILayout.BeginVertical(paddingStyle);
                DrawChannels();
                EditorGUILayout.Space(6f);
                DrawOutput(Mathf.Min(position.width - 40f, 320f));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
            }
            if (docked)
                FitToContent(0f);
        }

        // A floating window is sized to its contents, with its min and max size set to match so it can't be
        // resized by hand. Taller content than the screen fits scrolls instead. Docked windows are sized by
        // their dock area, so they get the normal limits back.
        void FitToContent(float windowHeight){
            Vector2 size;
            if (docked)
                size = DockedMinSize;
            else {
                float screenHeight = Screen.currentResolution.height / EditorGUIUtility.pixelsPerPoint - 100f;
                size = new Vector2(FloatingWidth, Mathf.Min(Mathf.Ceil(windowHeight), screenHeight));
            }
            // Sizes are compared within a pixel, since Unity can round them at fractional display scaling
            bool fits = docked
                ? minSize == DockedMinSize && maxSize == DockedMaxSize
                : Near(position.size, size) && Near(minSize, size) && Near(maxSize, size);
            if (fits || resizeQueued)
                return;
            // Resizing during OnGUI would change the layout mid-pass
            resizeQueued = true;
            EditorApplication.delayCall += () => {
                if (this == null)
                    return;
                resizeQueued = false;
                // Changing the size limits can move a floating window, so keep where it was
                Vector2 windowPos = position.position;
                if (docked){
                    minSize = DockedMinSize;
                    maxSize = DockedMaxSize;
                    return;
                }
                // Unity clamps each limit against the other, so the one that moves away from the current size goes first
                if (size.y > maxSize.y || size.x > maxSize.x){
                    maxSize = size;
                    minSize = size;
                }
                else {
                    minSize = size;
                    maxSize = size;
                }
                position = new Rect(windowPos, size);
            };
        }

        #region Toolbar and presets
        void DrawToolbar(){
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUIContent presetContent = new GUIContent("Preset: " + presetName, presetTip.tooltip);
            Rect presetRect = GUILayoutUtility.GetRect(presetContent, EditorStyles.toolbarDropDown, GUILayout.MaxWidth(240f));
            if (EditorGUI.DropdownButton(presetRect, presetContent, FocusType.Passive, EditorStyles.toolbarDropDown))
                ShowPresetMenu(presetRect);
            // These change which controls the channels draw, so the rest of this GUI pass is skipped
            if (GUILayout.Button(fillFromSelectionLabel, EditorStyles.toolbarButton)){
                FillFromSelection();
                GUIUtility.ExitGUI();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(resetLabel, EditorStyles.toolbarButton)){
                ResetAll();
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button(helpIcon, EditorStyles.toolbarButton))
                Application.OpenURL(WikiURL);
            EditorGUILayout.EndHorizontal();
        }

        void ShowPresetMenu(Rect rect){
            GenericMenu menu = new GenericMenu();
            foreach (PackerPreset preset in PackerPresets.BuiltIn){
                PackerPreset p = preset;
                menu.AddItem(new GUIContent(p.name), p.name == presetName, () => ApplyPreset(p, true, true));
            }
            List<PackerPreset> userPresets = PackerPresets.LoadUser();
            if (userPresets.Count > 0){
                menu.AddSeparator("");
                foreach (PackerPreset preset in userPresets){
                    PackerPreset p = preset;
                    menu.AddItem(new GUIContent(p.name), p.name == presetName, () => ApplyPreset(p, true, true));
                }
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Save Preset..."), false, StartNamingPreset);
            if (userPresets.Count > 0){
                foreach (PackerPreset preset in userPresets){
                    string name = preset.name;
                    menu.AddItem(new GUIContent("Delete Preset/" + name), false, () => DeletePreset(name));
                }
            }
            else
                menu.AddDisabledItem(new GUIContent("Delete Preset"));
            menu.DropDown(rect);
        }

        // Applies the preset's layout. With carryTextures, assigned textures move to the channel with the same name
        // (roughness and smoothness swap places inverted), or stay put if no names match, keeping their settings.
        void ApplyPreset(PackerPreset preset, bool carryTextures, bool recordUndo){
            if (recordUndo)
                Undo.RecordObject(this, "Apply Texture Packer Preset");
            PackerChannel[] previous = channels;
            PackerChannel[] result = new PackerChannel[4];
            for (int i = 0; i < 4; i++){
                result[i] = preset.channels[i].Clone();
                result[i].texture = null;
            }

            if (carryTextures && previous != null && previous.Length == 4){
                bool[] moved = new bool[4];
                bool anyMatch = false;
                for (int match = 2; match >= 1; match--){
                    for (int i = 0; i < 4; i++){
                        if (result[i].texture != null)
                            continue;
                        for (int j = 0; j < 4; j++){
                            PackerChannel old = previous[j];
                            if (moved[j] || old == null || old.texture == null || PackerPresets.MatchChannel(old.name, result[i].name) != match)
                                continue;
                            result[i].CopySource(old);
                            if (match == 1)
                                result[i].invert = !result[i].invert;
                            moved[j] = true;
                            anyMatch = true;
                            break;
                        }
                    }
                }
                if (!anyMatch){
                    for (int i = 0; i < 4; i++){
                        if (previous[i] != null && previous[i].texture != null)
                            result[i].CopySource(previous[i]);
                    }
                }
            }

            channels = result;
            includeAlpha = preset.includeAlpha || result[3].texture != null;
            sRGB = preset.sRGB;
            suffix = preset.suffix ?? "";
            presetName = preset.name;
            QueueRefresh();
            Repaint();
        }

        void ResetAll(){
            ApplyPreset(PackerPresets.Find(presetName) ?? PackerPresets.Custom, false, true);
            output = null;
            showFiltering = new bool[4];
        }

        void StartNamingPreset(){
            namingPreset = true;
            focusPresetName = true;
            newPresetName = PackerPresets.IsBuiltIn(presetName) ? "" : presetName;
            Repaint();
        }

        void DrawPresetNameBar(){
            Event e = Event.current;
            bool focused = GUI.GetNameOfFocusedControl() == "PresetName";
            bool submit = focused && e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter);
            bool cancel = focused && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Preset Name", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
            GUI.SetNextControlName("PresetName");
            newPresetName = EditorGUILayout.TextField(newPresetName, EditorStyles.toolbarTextField);
            if (focusPresetName && e.type == EventType.Repaint){
                EditorGUI.FocusTextInControl("PresetName");
                focusPresetName = false;
            }
            bool save = GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50f)) || submit;
            bool close = GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.Width(55f)) || cancel;
            EditorGUILayout.EndHorizontal();

            if (save || close){
                if (save){
                    string name = newPresetName;
                    EditorApplication.delayCall += () => SavePreset(name);
                }
                namingPreset = false;
                GUI.FocusControl(null);
                if (e.type == EventType.KeyDown)
                    e.Use();
                Repaint();
            }
        }

        // Saves the current channels and output settings, without their textures
        void SavePreset(string name){
            if (this == null)
                return;
            name = (name ?? "").Replace('/', '-').Trim();
            if (name.Length == 0){
                ShowNotification(new GUIContent("Enter a name for the preset"));
                return;
            }
            if (PackerPresets.IsBuiltIn(name)){
                ShowNotification(new GUIContent("A built-in preset is named " + name));
                return;
            }
            List<PackerPreset> userPresets = PackerPresets.LoadUser();
            int existing = userPresets.FindIndex(p => p.name == name);
            if (existing >= 0 && !EditorUtility.DisplayDialog("Texture Packer", "Replace the preset \"" + name + "\"?", "Replace", "Cancel"))
                return;

            PackerChannel[] saved = channels.Select(c => c.Clone()).ToArray();
            foreach (PackerChannel c in saved)
                c.texture = null;
            PackerPreset preset = new PackerPreset(name, suffix, sRGB, includeAlpha, saved);
            if (existing >= 0)
                userPresets[existing] = preset;
            else
                userPresets.Add(preset);
            PackerPresets.SaveUser(userPresets);
            presetName = name;
            ShowNotification(new GUIContent("Saved preset " + name));
            Repaint();
        }

        void DeletePreset(string name){
            if (!EditorUtility.DisplayDialog("Texture Packer", "Delete the preset \"" + name + "\"?", "Delete", "Cancel"))
                return;
            List<PackerPreset> userPresets = PackerPresets.LoadUser();
            userPresets.RemoveAll(p => p.name == name);
            PackerPresets.SaveUser(userPresets);
        }

        // Assigns the selected textures to the channels named after their maps. Textures naming more than one
        // channel (like "MetallicSmoothness") are left out, and roughness fills smoothness inverted (and the reverse).
        void FillFromSelection(){
            Texture[] selected = Selection.GetFiltered<Texture>(SelectionMode.Assets | SelectionMode.DeepAssets)
                .Where(t => t.dimension == TextureDimension.Tex2D)
                .OrderBy(t => t.name)
                .ToArray();
            if (selected.Length == 0){
                ShowNotification(new GUIContent("Select textures in the Project window first"));
                return;
            }

            string[] names = channels.Select(c => c.name).Distinct().ToArray();
            List<Texture> candidates = selected.Where(t => names.Count(n => PackerPresets.MatchTexture(t.name, n) == 2) <= 1).ToList();
            Texture[] picked = new Texture[4];
            bool[] inverted = new bool[4];
            for (int match = 2; match >= 1; match--){
                for (int i = 0; i < 4; i++){
                    if (picked[i] != null)
                        continue;
                    Texture tex = candidates.FirstOrDefault(t => !picked.Contains(t) && PackerPresets.MatchTexture(t.name, channels[i].name) == match);
                    if (tex == null)
                        continue;
                    picked[i] = tex;
                    inverted[i] = match == 1;
                }
            }

            int count = picked.Count(t => t != null);
            if (count == 0){
                ShowNotification(new GUIContent("None of the selected textures are named after a channel"));
                return;
            }
            Undo.RecordObject(this, "Fill Texture Packer From Selection");
            for (int i = 0; i < 4; i++){
                if (picked[i] == null)
                    continue;
                channels[i].texture = picked[i];
                channels[i].invert = inverted[i];
                if (i == 3)
                    includeAlpha = true;
            }
            ShowNotification(new GUIContent("Assigned " + count + (count == 1 ? " texture" : " textures")));
            QueueRefresh();
        }
        #endregion

        #region Channels
        void DrawChannels(){
            for (int i = 0; i < 4; i++)
                DrawChannel(i);
        }

        // Layout decisions use the state from before this event, since controls can't appear or disappear between Layout and the event that changed them
        void DrawChannel(int i){
            PackerChannel original = channels[i];
            PackerChannel c = original.Clone();
            bool isAlpha = i == 3;
            bool active = PackerRenderer.IsActive(i, includeAlpha);
            bool hasTexture = original.texture != null;

            EditorGUI.BeginChangeCheck();
            Rect card = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            Rect header = EditorGUILayout.GetControlRect(false, 20f);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(header.x - 2f, header.y - 1f, header.width + 4f, header.height + 2f), HeaderColor);
            Rect badge = new Rect(header.x + 1f, header.y + 1f, 18f, 18f);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(badge, ChannelColors[i]);
            GUI.Label(badge, ChannelLetters[i], badgeStyle);
            Rect menuRect = new Rect(header.xMax - 20f, header.y + 1f, 20f, 18f);
            float nameEnd = menuRect.x - 6f;
            if (isAlpha){
                Rect includeRect = new Rect(menuRect.x - 70f, header.y + 1f, 66f, 18f);
                EditorGUI.BeginChangeCheck();
                bool include = EditorGUI.ToggleLeft(includeRect, includeLabel, includeAlpha);
                if (EditorGUI.EndChangeCheck()){
                    Undo.RecordObject(this, include ? "Include Alpha" : "Leave Out Alpha");
                    includeAlpha = include;
                    QueueRefresh();
                }
                nameEnd = includeRect.x - 6f;
            }
            Rect nameRect = new Rect(badge.xMax + 6f, header.y + 1f, Mathf.Clamp(nameEnd - badge.xMax - 6f, 20f, 180f), 18f);
            c.name = EditorGUI.TextField(nameRect, c.name, nameStyle);
            GUI.Label(nameRect, nameTip);
            if (GUI.Button(menuRect, menuIcon, EditorStyles.label))
                ShowChannelMenu(i);

            if (active){
                EditorGUILayout.BeginHorizontal();
                Rect thumbnail = GUILayoutUtility.GetRect(ThumbnailSize, ThumbnailSize, GUILayout.Width(ThumbnailSize), GUILayout.Height(ThumbnailSize));
                DrawThumbnail(thumbnail, i);
                GUILayout.Space(4f);
                EditorGUILayout.BeginVertical();
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 58f;
                c.texture = TextureField(c.texture);
                if (hasTexture){
                    Rect row = EditorGUILayout.GetControlRect();
                    Rect invertRect = new Rect(row.xMax - 58f, row.y, 58f, row.height);
                    row.width -= 64f;
                    c.source = (PackerSource)EditorGUI.EnumPopup(row, sourceLabel, c.source);
                    c.invert = EditorGUI.ToggleLeft(invertRect, invertLabel, c.invert);
                    EditorGUILayout.LabelField(GetSourceInfo(original.texture), EditorStyles.miniLabel);
                }
                else
                    c.fill = EditorGUILayout.Slider(fillLabel, c.fill, 0f, 1f);
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                if (hasTexture)
                    DrawFiltering(i, original, c);
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck()){
                Undo.RecordObject(this, "Edit Texture Packer Channel");
                channels[i] = c;
                QueueRefresh();
            }
            HandleDragAndDrop(card, i);
            GUILayout.Space(2f);
        }

        // Texture fields draw as a large thumbnail picker unless their height is set
        Texture TextureField(Texture tex){
            Texture picked = (Texture)EditorGUILayout.ObjectField(textureLabel, tex, typeof(Texture), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (picked != null && picked != tex && picked.dimension != TextureDimension.Tex2D){
                ShowNotification(new GUIContent("Only 2D textures can be packed"));
                return tex;
            }
            return picked;
        }

        void DrawThumbnail(Rect rect, int i){
            Event e = Event.current;
            if (e.type == EventType.Repaint){
                EditorGUI.DrawRect(rect, PreviewBackground);
                RenderTexture tex = packer.GetThumbnail(i);
                if (tex != null)
                    EditorGUI.DrawPreviewTexture(FitRect(rect, tex.width, tex.height), tex);
                if (GetShownView() == i + 1)
                    DrawOutline(rect, ChannelColors[i]);
            }
            else if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition)){
                view = view == i + 1 ? 0 : i + 1;
                QueueRefresh();
                e.Use();
            }
            GUI.Label(rect, thumbnailTip);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
        }

        void DrawFiltering(int i, PackerChannel original, PackerChannel c){
            bool show = showFiltering[i];
            bool modified = original.HasFiltering();
            Rect row = EditorGUILayout.GetControlRect();
            Rect foldoutRect = row;
            if (modified)
                foldoutRect.width -= 56f;

            // Folding isn't an edit, so it shouldn't trigger the channel's change check
            bool changed = GUI.changed;
            showFiltering[i] = EditorGUI.Foldout(foldoutRect, show, filteringLabel, true);
            GUI.changed = changed;
            if (modified && GUI.Button(new Rect(row.xMax - 52f, row.y + 1f, 52f, row.height - 1f), resetFilteringLabel, EditorStyles.miniButton)){
                c.ResetFiltering();
                GUI.changed = true;
            }
            if (!show)
                return;

            EditorGUI.indentLevel++;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 110f;
            // Same order as PackChannel in the shader
            c.inputLevels = LevelsField(inputLevelsLabel, c.inputLevels);
            c.gamma = EditorGUILayout.Slider(gammaLabel, c.gamma, 0.1f, 5f);
            c.contrast = EditorGUILayout.Slider(contrastLabel, c.contrast, 0f, 2f);
            c.brightness = EditorGUILayout.Slider(brightnessLabel, c.brightness, 0f, 2f);
            c.outputLevels = LevelsField(outputLevelsLabel, c.outputLevels);
            c.sampling = (PackerSampling)EditorGUILayout.EnumPopup(samplingLabel, c.sampling);
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(2f);
        }

        // A min/max slider between two value fields, limited to 0-1 with min never above max
        static Vector2 LevelsField(GUIContent label, Vector2 range){
            Rect rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            const float fieldWidth = 38f;
            Rect minRect = new Rect(rect.x, rect.y, fieldWidth, rect.height);
            Rect maxRect = new Rect(rect.xMax - fieldWidth, rect.y, fieldWidth, rect.height);
            Rect sliderRect = new Rect(minRect.xMax + 4f, rect.y, Mathf.Max(0f, rect.width - fieldWidth * 2f - 8f), rect.height);
            float min = range.x;
            float max = range.y;
            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, 0f, 1f);
            if (EditorGUI.EndChangeCheck()){
                min = Mathf.Round(min * 1000f) / 1000f;
                max = Mathf.Round(max * 1000f) / 1000f;
            }
            min = EditorGUI.FloatField(minRect, min);
            max = EditorGUI.FloatField(maxRect, max);
            EditorGUI.indentLevel = indent;
            max = Mathf.Clamp01(max);
            min = Mathf.Clamp(min, 0f, max);
            return new Vector2(min, max);
        }

        void HandleDragAndDrop(Rect rect, int i){
            Event e = Event.current;
            if ((e.type != EventType.DragUpdated && e.type != EventType.DragPerform) || !rect.Contains(e.mousePosition))
                return;
            Texture tex = DragAndDrop.objectReferences.OfType<Texture>().FirstOrDefault(t => t.dimension == TextureDimension.Tex2D);
            if (tex == null)
                return;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (e.type == EventType.DragPerform){
                DragAndDrop.AcceptDrag();
                Undo.RecordObject(this, "Assign Texture Packer Texture");
                channels[i].texture = tex;
                if (i == 3)
                    includeAlpha = true;
                QueueRefresh();
            }
            e.Use();
        }

        void ShowChannelMenu(int i){
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy Channel"), false, () => clipboard = channels[i].Clone());
            if (clipboard != null)
                menu.AddItem(new GUIContent("Paste Channel"), false, () => EditChannels("Paste Texture Packer Channel", () => {
                    string name = channels[i].name;
                    channels[i] = clipboard.Clone();
                    channels[i].name = name;
                    IncludeAlphaIfAssigned(i);
                }));
            else
                menu.AddDisabledItem(new GUIContent("Paste Channel"));
            menu.AddSeparator("");
            for (int j = 0; j < 4; j++){
                if (j == i)
                    continue;
                int other = j;
                menu.AddItem(new GUIContent("Swap With/" + ChannelLetters[j] + " (" + channels[j].name.Replace('/', '-') + ")"), false, () => EditChannels("Swap Texture Packer Channels", () => {
                    PackerChannel first = channels[i].Clone();
                    channels[i].CopySource(channels[other]);
                    channels[other].CopySource(first);
                    IncludeAlphaIfAssigned(i);
                    IncludeAlphaIfAssigned(other);
                }));
            }
            if (channels[i].texture != null)
                menu.AddItem(new GUIContent("Use Texture in Every Channel"), false, () => EditChannels("Use Texture in Every Channel", () => {
                    Texture tex = channels[i].texture;
                    for (int k = 0; k < 4; k++){
                        channels[k].texture = tex;
                        channels[k].source = (PackerSource)k;
                    }
                }));
            else
                menu.AddDisabledItem(new GUIContent("Use Texture in Every Channel"));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Reset Channel"), false, () => EditChannels("Reset Texture Packer Channel", () => {
                PackerPreset preset = PackerPresets.Find(presetName);
                PackerChannel reset = preset != null ? preset.channels[i].Clone() : new PackerChannel(channels[i].name, channels[i].fill);
                reset.texture = null;
                reset.name = channels[i].name;
                channels[i] = reset;
            }));
            menu.ShowAsContext();
        }

        void EditChannels(string undoName, Action edit){
            Undo.RecordObject(this, undoName);
            edit();
            QueueRefresh();
            Repaint();
        }

        // A texture moved into the alpha channel turns it on, so it isn't silently left out
        void IncludeAlphaIfAssigned(int channel){
            if (channel == 3 && channels[3].texture != null)
                includeAlpha = true;
        }

        string GetSourceInfo(Texture tex){
            Vector2Int size = GetSourceSize(tex);
            string ext = Path.GetExtension(AssetDatabase.GetAssetPath(tex)).TrimStart('.').ToUpperInvariant();
            return size.x + " × " + size.y + (ext.Length > 0 ? "  " + ext : "");
        }
        #endregion

        #region Output
        void DrawOutput(float previewSize){
            Vector2Int size = GetOutputSize();
            int shownView = GetShownView();
            bool alpha = includeAlpha;
            bool custom = sizeIndex == CustomSizeIndex;
            string target = GetTargetPath();

            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            Rect box = EditorGUILayout.GetControlRect(false, previewSize);
            if (Event.current.type == EventType.Repaint){
                EditorGUI.DrawRect(box, PreviewBackground);
                if (packer.Display != null)
                    EditorGUI.DrawPreviewTexture(FitRect(box, size.x, size.y), packer.Display);
            }

            Rect viewRow = EditorGUILayout.GetControlRect();
            float buttonWidth = viewRow.width / ViewLabels.Length;
            for (int v = 0; v < ViewLabels.Length; v++){
                GUIStyle style = v == 0 ? EditorStyles.miniButtonLeft : v == ViewLabels.Length - 1 ? EditorStyles.miniButtonRight : EditorStyles.miniButtonMid;
                EditorGUI.BeginDisabledGroup(v == 4 && !alpha);
                if (GUI.Toggle(new Rect(viewRow.x + buttonWidth * v, viewRow.y, buttonWidth, viewRow.height), shownView == v, ViewLabels[v], style) && shownView != v){
                    view = v;
                    QueueRefresh();
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.LabelField(GetSizeInfo(size, alpha), EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(4f);

            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60f;
            EditorGUI.BeginChangeCheck();
            int newSizeIndex = EditorGUILayout.Popup(sizeLabel, sizeIndex, SizeLabels);
            int newWidth = customWidth;
            int newHeight = customHeight;
            if (custom){
                Rect row = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), new GUIContent(" "));
                float fieldWidth = (row.width - 16f) / 2f;
                newWidth = EditorGUI.DelayedIntField(new Rect(row.x, row.y, fieldWidth, row.height), customWidth);
                GUI.Label(new Rect(row.x + fieldWidth, row.y, 16f, row.height), "×", EditorStyles.centeredGreyMiniLabel);
                newHeight = EditorGUI.DelayedIntField(new Rect(row.xMax - fieldWidth, row.y, fieldWidth, row.height), customHeight);
            }
            PackerFormat newFormat = (PackerFormat)EditorGUILayout.EnumPopup(formatLabel, format);
            bool newSRGB = EditorGUILayout.ToggleLeft(sRGBLabel, sRGB);
            if (EditorGUI.EndChangeCheck()){
                Undo.RecordObject(this, "Edit Texture Packer Output");
                // Custom starts from the current size rather than jumping to the last custom one
                if (newSizeIndex == CustomSizeIndex && !custom){
                    newWidth = size.x;
                    newHeight = size.y;
                }
                sizeIndex = newSizeIndex;
                customWidth = Mathf.Clamp(newWidth, 1, MaxSize);
                customHeight = Mathf.Clamp(newHeight, 1, MaxSize);
                format = newFormat;
                sRGB = newSRGB;
                QueueRefresh();
            }

            EditorGUI.BeginChangeCheck();
            Texture2D newOutput = (Texture2D)EditorGUILayout.ObjectField(outputLabel, output, typeof(Texture2D), false, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (EditorGUI.EndChangeCheck())
                SetOutput(newOutput);
            EditorGUIUtility.labelWidth = labelWidth;

            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(packAsLabel, GUILayout.Height(28f)))
                EditorApplication.delayCall += () => Pack(true);
            GUIContent packLabel = target != null
                ? new GUIContent("Pack", "Packs into " + Path.GetFileName(target) + ", replacing it.")
                : new GUIContent("Pack...", "Packs into a new file.");
            if (GUILayout.Button(packLabel, GUILayout.Height(28f)))
                EditorApplication.delayCall += () => Pack(false);
            EditorGUILayout.EndHorizontal();
            if (target != null)
                EditorGUILayout.LabelField("Pack replaces " + Path.GetFileName(target), EditorStyles.centeredGreyMiniLabel);
        }

        void SetOutput(Texture2D tex){
            if (tex != null && GetPackableExtension(AssetDatabase.GetAssetPath(tex)) == null){
                ShowNotification(new GUIContent("Only PNG and TGA files in Assets can be packed into"));
                return;
            }
            Undo.RecordObject(this, "Set Texture Packer Output");
            output = tex;
            if (tex != null)
                format = GetPackableExtension(AssetDatabase.GetAssetPath(tex)) == ".tga" ? PackerFormat.TGA : PackerFormat.PNG;
        }

        static string GetPackableExtension(string path){
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal))
                return null;
            string ext = Path.GetExtension(path).ToLowerInvariant();
            return ext == ".png" || ext == ".tga" ? ext : null;
        }

        // The output texture's path, if it's a file Pack can overwrite in the chosen format
        string GetTargetPath(){
            if (output == null)
                return null;
            string path = AssetDatabase.GetAssetPath(output);
            return GetPackableExtension(path) == GetFormatExtension() ? path : null;
        }

        string GetFormatExtension(){
            return format == PackerFormat.TGA ? ".tga" : ".png";
        }

        void Pack(bool chooseLocation){
            if (this == null || packer == null || !packer.IsValid)
                return;
            string path = chooseLocation ? null : GetTargetPath();
            if (path == null){
                path = EditorUtility.SaveFilePanelInProject("Pack Texture", GetDefaultName(), GetFormatExtension().TrimStart('.'), "Choose where to save the packed texture.", GetDefaultFolder());
                if (string.IsNullOrEmpty(path))
                    return;
            }
            foreach (Texture tex in GetActiveTextures()){
                if (AssetDatabase.GetAssetPath(tex) == path){
                    EditorUtility.DisplayDialog("Texture Packer", Path.GetFileName(path) + " is one of the source textures, so it can't be packed into. Choose a different file.", "OK");
                    return;
                }
            }

            Texture2D packed = null;
            try {
                packed = packer.Bake(channels, includeAlpha, GetOutputSize());
                EditorUtility.DisplayProgressBar("Texture Packer", "Saving " + Path.GetFileName(path) + "...", 1f);
                Texture2D saved = PackerRenderer.Save(packed, path, includeAlpha, sRGB);
                if (saved == null)
                    throw new Exception("couldn't load the packed texture at " + path + ".");
                output = saved;
                EditorGUIUtility.PingObject(saved);
                ShowNotification(new GUIContent("Packed " + Path.GetFileName(path)));
            }
            catch (Exception e){
                Debug.LogError("[Mochie] Texture Packer: packing " + path + " failed.");
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Texture Packer", "Packing failed: " + e.Message + "\n\nThe Console has the details.", "OK");
            }
            finally {
                EditorUtility.ClearProgressBar();
                if (packed != null)
                    DestroyImmediate(packed);
            }
            Repaint();
        }

        string GetDefaultName(){
            if (output != null)
                return Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(output));
            string name = PackerPresets.GetBaseName(GetActiveTextures().Select(t => t.name)) + suffix;
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        // Next to the current output, or else the first source texture in Assets
        string GetDefaultFolder(){
            string path = output != null ? AssetDatabase.GetAssetPath(output) : GetActiveTextures().Select(AssetDatabase.GetAssetPath).FirstOrDefault(p => p.StartsWith("Assets/", StringComparison.Ordinal));
            return string.IsNullOrEmpty(path) ? "Assets" : Path.GetDirectoryName(path).Replace('\\', '/');
        }

        IEnumerable<Texture> GetActiveTextures(){
            return channels.Where((c, i) => PackerRenderer.IsActive(i, includeAlpha) && c.texture != null).Select(c => c.texture).Distinct();
        }

        Vector2Int GetSourceSize(Texture tex){
            if (!sourceSizes.TryGetValue(tex, out Vector2Int size)){
                size = PackerRenderer.GetSourceSize(tex);
                sourceSizes[tex] = size;
            }
            return size;
        }

        Vector2Int GetOutputSize(){
            if (sizeIndex >= 1 && sizeIndex <= Sizes.Length)
                return new Vector2Int(Sizes[sizeIndex - 1], Sizes[sizeIndex - 1]);
            if (sizeIndex == CustomSizeIndex)
                return new Vector2Int(Mathf.Clamp(customWidth, 1, MaxSize), Mathf.Clamp(customHeight, 1, MaxSize));

            // Auto uses the largest source by pixel count, or 1024 when every channel is a fill value
            Vector2Int largest = new Vector2Int(1024, 1024);
            long largestPixels = 0;
            foreach (Texture tex in GetActiveTextures()){
                Vector2Int size = GetSourceSize(tex);
                long pixels = (long)size.x * size.y;
                if (pixels > largestPixels){
                    largestPixels = pixels;
                    largest = size;
                }
            }
            return new Vector2Int(Mathf.Min(largest.x, MaxSize), Mathf.Min(largest.y, MaxSize));
        }

        int GetShownView(){
            return view == 4 && !includeAlpha ? 0 : view;
        }

        // Memory with mip maps, using the compression a new packed texture gets on PC
        static string GetSizeInfo(Vector2Int size, bool alpha){
            double kilobytes = (double)size.x * size.y * (alpha ? 1.0 : 0.5) * 4.0 / 3.0 / 1024.0;
            string memory = kilobytes >= 1024.0 ? (kilobytes / 1024.0).ToString("0.0") + " MB" : Math.Max(1.0, Math.Round(kilobytes)) + " KB";
            return size.x + " × " + size.y + " " + (alpha ? "RGBA" : "RGB") + ", about " + memory + " as " + (alpha ? "BC7" : "DXT1");
        }
        #endregion

        static bool Near(Vector2 a, Vector2 b){
            return Mathf.Abs(a.x - b.x) < 1f && Mathf.Abs(a.y - b.y) < 1f;
        }

        static Rect FitRect(Rect rect, float width, float height){
            float scale = Mathf.Min(rect.width / Mathf.Max(width, 1f), rect.height / Mathf.Max(height, 1f));
            Vector2 size = new Vector2(width * scale, height * scale);
            return new Rect(rect.center - size / 2f, size);
        }

        static void DrawOutline(Rect rect, Color color){
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 2f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 2f, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 2f, rect.y, 2f, rect.height), color);
        }
    }

    class TexturePackerPostprocessor : AssetPostprocessor {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths){
            if (importedAssets.Length > 0)
                TexturePackerWindow.OnAssetsImported(importedAssets);
        }
    }
}
