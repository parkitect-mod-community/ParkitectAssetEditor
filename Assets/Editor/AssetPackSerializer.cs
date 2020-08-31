using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ParkitectAssetEditor
{
    /// <summary>
    /// Help class for serializing the asset pack.
    /// </summary>
    public static class AssetPackSerializer
    {
        /// <summary>
        /// Saves the specified asset pack.
        /// </summary>
        /// <param name="assetPack">The asset pack.</param>
        /// <returns></returns>
        public static bool CreateAssetBundle(this AssetPack assetPack)
        {
            if (assetPack.Assets.Any(a => a.GameObject == null))
            {
                foreach (var asset in assetPack.Assets.Where(a => a.GameObject == null))
                {
                    Debug.LogError(string.Format("Could not save asset pack because GameObject of asset {0} is missing.", asset.Name));

                    return false;
                }
            }

            // make sure the prefab directory exists
            Directory.CreateDirectory(Path.Combine(ProjectManager.Project.Value.ProjectDirectory, "Resources/AssetPack"));

            // create the prefabs and store the paths in prefabPaths
            var prefabPaths = new List<string>();
            foreach (var asset in assetPack.Assets)
            {
                if (asset.TargetType == AssetType.Train) {
                    if (asset.LeadCar != null) {
                        prefabPaths.Add(CreatePrefab(asset.LeadCar.GameObject, asset.LeadCar.Guid));
                    }
                    if (asset.Car != null) {
                        prefabPaths.Add(CreatePrefab(asset.Car.GameObject, asset.Car.Guid));
                    }
                    if (asset.RearCar != null) {
                        prefabPaths.Add(CreatePrefab(asset.RearCar.GameObject, asset.RearCar.Guid));
                    }
                }
                else if (asset.TargetType == AssetType.Shop)
                {
                    foreach (var product in asset.Products)
                    {
                        prefabPaths.Add(CreatePrefab(product.Trash, product.TrashGuid));
                        prefabPaths.Add(CreatePrefab(product.Product, product.Guid));
                    }
                    prefabPaths.Add(CreatePrefab(asset.GameObject, asset.Guid));
                }
                else if (asset.TargetType == AssetType.Path)
                {
                    prefabPaths.Add(CreateTexture(asset.PathSheet, asset.Guid + ".path_sheet"));
                    prefabPaths.Add(CreateTexture(asset.PathMask, asset.Guid + ".path_mask"));
                    prefabPaths.Add(CreateTexture(asset.PathNormal, asset.Guid + ".path_normal"));
                }
                else {
                    asset.LeadCar = null;
                    asset.Car = null;
                    asset.RearCar = null;
                    prefabPaths.Add(CreatePrefab(asset.GameObject, asset.Guid));
                }
            }

            // use the prefab list to build an assetbundle
            AssetBundleBuild[] descriptor = {
                new AssetBundleBuild()
                {
                    assetBundleName = "assetPack",
                    assetNames      = prefabPaths.ToArray()
                }
            };

#if UNITY_STANDALONE_OSX
            BuildPipeline.BuildAssetBundles(ProjectManager.Project.Value.ModDirectory, descriptor, BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneOSX);
#elif UNITY_STANDALONE_WIN
            BuildPipeline.BuildAssetBundles(ProjectManager.Project.Value.ModDirectory, descriptor, BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneWindows);
#else
            BuildPipeline.BuildAssetBundles(ProjectManager.Project.Value.ModDirectory, descriptor, BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.StrictMode, BuildTarget.StandaloneLinux64);
#endif
            return true;
        }

        private static string CreatePrefab(GameObject gameObject, string Guid) {
            if (gameObject == null) {
                return null;
            }

            var path = string.Format("Assets/Resources/AssetPack/{0}.prefab", Guid);

            PrefabUtility.SaveAsPrefabAsset(gameObject,path);

            return path;
        }

        private static string CreateTexture(Texture2D texture2D, string Guid)
        {
            if (texture2D == null)
            {
                return null;
            }

            string currentTexturePath = AssetDatabase.GetAssetPath(texture2D);

            TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(currentTexturePath);
            importer.textureType = TextureImporterType.Default;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;

            AssetDatabase.ImportAsset( importer.assetPath );
            Texture2D inst = (Texture2D) AssetDatabase.LoadAssetAtPath(importer.assetPath, typeof(Texture2D));
            EditorUtility.SetDirty(inst);
            // Debug.Log(oldPath);
            //
            // Texture2D inst = Resources.Load<Texture2D>(oldPath);
            string path = string.Format("Assets/Resources/AssetPack/{0}.png", Guid);
            byte[] data = inst.EncodeToPNG();
            Debug.Log(data.Length / 1024 + "Kb was saved as: " + path);
            File.WriteAllBytes(path, inst.EncodeToPNG());
            return path;
        }


        /// <summary>
        /// Fills asset pack with gameobjects from the scene and/or prefabs.
        /// </summary>
        /// <param name="assetPack">The asset pack.</param>
        public static void LoadGameObjects(this AssetPack assetPack)
        {
            for (var i = assetPack.Assets.Count - 1; i >= 0; i--)
            {
                var asset = assetPack.Assets[i];

                // instantiate the prefab if game object doesn't exist.
                if (asset.GameObject == null)
                {
                    Debug.Log(string.Format("Can't find {0} in the scene, instantiating prefab.", asset.Name));
                    try // if one object fails to load, don't make it fail the rest
                    {
                        var go = Resources.Load<GameObject>(string.Format("AssetPack/{0}", asset.Guid));

                        asset.GameObject = Object.Instantiate(go);
                        asset.GameObject.name = asset.Name;
                    }
                    catch (System.Exception)
                    {
                        Debug.LogError(string.Format("Could not find GameObject at Assets/Resources/AssetPack/{0} for asset {1}, skipped loading of asset", asset.Guid, asset.Name));
                        // assetPack.Assets.Remove(asset);
                    }
                }

                if (asset.TargetType == AssetType.Train)
                {
                    if (asset.LeadCar != null && asset.LeadCar.GameObject == null)
                    {
                        asset.LeadCar.GameObject = LoadGameObject(asset.LeadCar.Guid, asset);
                    }

                    if (asset.Car != null && asset.Car.GameObject == null)
                    {
                        asset.Car.GameObject = LoadGameObject(asset.Car.Guid, asset);
                    }

                    if (asset.RearCar != null && asset.RearCar.GameObject == null)
                    {
                        asset.RearCar.GameObject = LoadGameObject(asset.RearCar.Guid, asset);
                    }
                }
            }
        }

        private static GameObject LoadGameObject(string Guid, Asset asset)
        {
            if (!string.IsNullOrEmpty(Guid))
            {
                try // if one object fails to load, don't make it fail the rest
                {
                    var go = Resources.Load<GameObject>(string.Format("AssetPack/{0}", Guid));

                    return Object.Instantiate(go);
                }
                catch (System.Exception)
                {
                    Debug.LogError(string.Format("Could not find GameObject at Assets/Resources/AssetPack/{0} for asset {1}, skipped loading of asset", Guid, asset.Name));

                    return null;
                }
            }

            return null;
        }
    }
}
