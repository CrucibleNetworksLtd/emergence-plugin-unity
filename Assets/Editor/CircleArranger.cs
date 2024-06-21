using UnityEngine;
using UnityEditor;

namespace EmergenceSDK.Editor
{
    public class CircleArranger : EditorWindow
    {
        private float _radius = 5.0f;
        private float _angle = 360.0f;

        [MenuItem("Tools/Circle Arranger")]
        static void OpenWindow()
        {
            GetWindow<CircleArranger>("Circle Arranger");
        }

        void OnGUI()
        {
            GUILayout.Label("Arrange selected GameObjects", EditorStyles.boldLabel);
            _radius = EditorGUILayout.FloatField("Radius", _radius);
            _angle = EditorGUILayout.FloatField("Angle", _angle);

            if (GUILayout.Button("Arrange"))
            {
                ArrangeObjects();
            }
        }

        void ArrangeObjects()
        {
            if (Selection.gameObjects.Length > 1)
            {
                Undo.SetCurrentGroupName("Circle Arrangement");
                int group = Undo.GetCurrentGroup();

                // Use local coordinates if the objects have a parent
                bool hasParent = Selection.gameObjects[0].transform.parent != null;
                Transform parentTransform = hasParent ? Selection.gameObjects[0].transform.parent : null;
                Vector3 center = hasParent ? parentTransform.position : Vector3.zero;
                float yCoordinate = hasParent ? 0 : Selection.gameObjects[0].transform.position.y;
                int count = Selection.gameObjects.Length;
                float angleStep = _angle / (count - (_angle < 360 ? 1 : 0));

                for (int i = 0; i < count; i++)
                {
                    GameObject go = Selection.gameObjects[i];
                    Undo.RecordObject(go.transform, "Move GameObject");
                    
                    float ang = i * angleStep * Mathf.Deg2Rad;
                    Vector3 pos = new Vector3(Mathf.Sin(ang), 0, Mathf.Cos(ang)) * _radius;
                    pos += center; // Set position relative to center
                    
                    if (hasParent) {
                        pos.y = parentTransform.position.y + (yCoordinate - parentTransform.position.y); // Maintain relative y-offset
                    } else {
                        pos.y = yCoordinate; // Use fixed y-coordinate if no parent
                    }

                    go.transform.position = pos;
                    go.transform.LookAt(new Vector3(center.x, pos.y, center.z)); // Adjust facing direction
                }

                Undo.CollapseUndoOperations(group);
            }
            else
            {
                Debug.LogWarning("Please select at least two GameObjects to arrange them in a circle.");
            }
        }
    }
}
