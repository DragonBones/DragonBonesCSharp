using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace DragonBones
{
    public class PickJsonDataWindow : EditorWindow
    {
        private const string ObjectSelectorUpdated = "ObjectSelectorUpdated";
        private const string ObjectSelectorClosed = "ObjectSelectorClosed";


        private UnityArmatureComponent _armatureComp;
        private TextAsset _dragonBoneJSONData;

        private bool _isOpenPickWindow = false;
        private int _controlID;

        //
        public static void OpenWindow(UnityArmatureComponent armatureComp)
        {
            var win = GetWindow<PickJsonDataWindow>();
            win._armatureComp = armatureComp;
        }

        private void OnDestroy()
        {
            _armatureComp = null;
            _dragonBoneJSONData = null;

            _isOpenPickWindow = false;
            _controlID = 0;
        }

        private void Awake()
        {
            _dragonBoneJSONData = null;

            _isOpenPickWindow = false;
            _controlID = 0;

            this.maxSize = Vector2.one;
            this.minSize = Vector2.one;
        }

        private void OnGUI()
        {
            ShowPickJsonWindow();

            string commandName = Event.current.commandName;
            if (commandName == ObjectSelectorUpdated)
            {
                //更新JSON数据
                _dragonBoneJSONData = EditorGUIUtility.GetObjectPickerObject() as TextAsset;
            }
            else if (commandName == ObjectSelectorClosed)
            {
                //根据选择的JSON数据设置DragonBonesData
                //UnityEditor.ChangeDragonBonesData(_armatureComp, _dragonBoneJSONData);

                List<string> textureAtlasJSONs = new List<string>();
                UnityEditor.GetTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(_dragonBoneJSONData.GetInstanceID()));
                UnityDragonBonesData.TextureAtlas[] textureAtlas = UnityEditor.GetTextureAtlasByJSONs(textureAtlasJSONs);

                UnityDragonBonesData data = UnityEditor.CreateUnityDragonBonesData(_dragonBoneJSONData, textureAtlas);
                _armatureComp.unityData = data;

                Repaint();

                this.Close();
            }
        }

        private void ShowPickJsonWindow()
        {
            if (_isOpenPickWindow)
            {
                return;
            }

            _controlID = EditorGUIUtility.GetControlID(FocusType.Passive);
            string filters = "_ske t:TextAsset";
            EditorGUIUtility.ShowObjectPicker<TextAsset>(null, false, filters, _controlID);

            _isOpenPickWindow = true;
        }
    }
}
