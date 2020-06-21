using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor.GizmoRenderers
{
    public class CarRenderer : IGizmoRenderer
    {
        private Material sceneViewMaterial;

        public bool CanRender(Asset asset)
        {
            return asset.Type == AssetType.Car;
        }

        public void Render(Asset asset)
        {
            if (sceneViewMaterial == null)
            {
                sceneViewMaterial =
                    (Material) AssetDatabase.LoadAssetAtPath("Assets/Editor/SceneViewGhostMaterial.mat",
                        typeof(Material));
            }

            CoasterCar car = asset.Car;
            if (car == null)
            {
                return;
            }

            Color gizmoColor = Gizmos.color;
            Gizmos.color = Color.red;
            var seats = asset.GameObject.GetComponentsInChildren<Transform>(true).Where(transform =>
                transform.name.StartsWith("Seat", true, CultureInfo.InvariantCulture));

            foreach (Transform seatTransform in seats)
            {
                Vector3 position = seatTransform.position + seatTransform.forward * car.SeatWaypointOffset -
                                   Vector3.up * 0.06f;
                Gizmos.DrawSphere(position, 0.01f);
            }

            Gizmos.color = gizmoColor;

            Gizmos.color = Color.white;
            Vector3 frontPosition = asset.GameObject.transform.position +
                                    asset.GameObject.transform.forward * car.OffsetFront;
            Gizmos.DrawLine(frontPosition, frontPosition + Vector3.up * 0.5f);
            Handles.Label(frontPosition + Vector3.up * 0.5f + asset.GameObject.transform.forward * 0.1f, "Front");

            Transform backAxis = asset.GameObject.transform.Find("backAxis");
            if (backAxis != null)
            {
                Vector3 backPosition = backAxis.position;
                backPosition -= asset.GameObject.transform.forward * car.OffsetBack;

                Gizmos.DrawLine(backPosition, backPosition + Vector3.up * 0.5f);
                Handles.Label(backPosition - Vector3.up * 0.1f - asset.GameObject.transform.forward * 0.1f, "Back");
            }

            foreach (CoasterRestraints restraints in car.Restraints)
            {
                var restraintTransforms = asset.GameObject.GetComponentsInChildren<Transform>(true).Where(transform =>
                    transform.name.StartsWith(restraints.TransformName, true, CultureInfo.InvariantCulture));

                foreach (Transform restraintTransform in restraintTransforms)
                {
                    MeshFilter meshFilter = restraintTransform.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        sceneViewMaterial.SetPass(0);
                        Graphics.DrawMeshNow(meshFilter.sharedMesh, restraintTransform.position,
                            restraintTransform.rotation * Quaternion.Euler(restraints.ClosedAngle, 0, 0));
                        sceneViewMaterial.SetPass(1);
                        Graphics.DrawMeshNow(meshFilter.sharedMesh, restraintTransform.position,
                            restraintTransform.rotation * Quaternion.Euler(restraints.ClosedAngle, 0, 0));
                    }
                }
            }
        }
    }
}
