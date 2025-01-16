using UnityEngine;
using UnityEditor;

namespace BIMOS
{
    [ExecuteInEditMode]
    [CustomEditor(typeof(GrabHandler))]
    public class HandSimulatorEditor : Editor
    {
        private GrabHandler _hand;
        private Grab _selectedGrab;

        private void OnEnable()
        {
            _hand = (GrabHandler)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pose Adjustment", EditorStyles.boldLabel);

            _selectedGrab = (Grab)EditorGUILayout.ObjectField("Grab Object", _selectedGrab, typeof(Grab), true);

            if (_selectedGrab != null)
            {
                if (GUILayout.Button("Simulate Grab"))
                {
                    SimulateGrab();
                }
            }

            if (_hand._hand.CurrentGrab != null)
            {
                if (GUILayout.Button("Release Object"))
                {
                    SimulateRelease();
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Changes will persist only in the Editor mode.", MessageType.Info);
            }
        }

        private void SimulateGrab()
        {
            if (_selectedGrab == null)
            {
                Debug.LogWarning("No Grab object selected!");
                return;
            }

            _hand._hand.CurrentGrab = _selectedGrab;

            // Simula el agarre llamando a OnGrab
            _selectedGrab.OnGrab(_hand);

            // Ajusta la pose inicial
            if (_selectedGrab.HandPose != null)
            {
                _hand._hand.GrabHandler.ApplyGrabPose(_selectedGrab.HandPose);
            }

            Debug.Log($"Simulated grab on object: {_selectedGrab.name}");
        }

        private void SimulateRelease()
        {
            if (_hand._hand.CurrentGrab == null)
            {
                Debug.LogWarning("No object is currently grabbed!");
                return;
            }

            // Llama a OnRelease del objeto agarrado
             _hand._hand.CurrentGrab.OnRelease(_hand, true);
            _hand._hand.CurrentGrab = null;

            Debug.Log("Simulated release of the current object.");
        }
    }
}
