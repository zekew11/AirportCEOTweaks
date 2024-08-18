using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System.Reflection;
using Tweaks_PerformanceCEO;
using AirportCEOTweaksCore;



namespace AirportCEOAircraft
{
    class AircraftAdder : MonoBehaviour
    {
        public bool working = true;

        public IEnumerator Initilize()
        {
            Debug.Log("Tweaks Aircraft loader init");
            List<AircraftTypeData> aircraftTypeList = ProccessAircraftPaths(AirportCEOAircraft.aircraftPaths.ToArray());
            AirTrafficController atc = Singleton<AirTrafficController>.Instance;
            HashSet<GameObject> aircraftGameObjectsSet = new HashSet<GameObject>();

            int processedAircraftCount = 1;
            foreach (AircraftTypeData aircraftTypeData in aircraftTypeList)
            {
                Debug.Log("Tweaks Aircraft loader foreach "+ aircraftTypeData.Id);
                for (int i = 0; i<aircraftTypeData.id.Length; i++)
                {
                    GameObject aircraftGameObject = MakeAircraftGameObject(aircraftTypeData, i);

                    DoTweaksLiveryBakeIn(aircraftGameObject, aircraftTypeData); //must proceed scaling becasue scale is based off bounding box

                    AircraftScaleManager scale;
                    if(!aircraftGameObject.TryGetComponent<AircraftScaleManager>(out scale))
                    {
                        scale = aircraftGameObject.AddComponent<AircraftScaleManager>();
                    }
                    scale.forcedScale = aircraftTypeData.forcedReScale;
                    scale.wingspan = aircraftTypeData.wingSpan_M;
                    scale.length = aircraftTypeData.length_M;
                    scale.Init();

                    aircraftGameObjectsSet.Add(aircraftGameObject);

                    if (!AirportCEOTweaksCore.AirportCEOTweaksCore.aircraftTypeDataDict.ContainsKey(aircraftTypeData.id[i]))
                    {
                        AirportCEOTweaksCore.AirportCEOTweaksCore.aircraftTypeDataDict.Add(aircraftTypeData.id[i], aircraftTypeData.SingleAircraftTypeData(aircraftTypeData.id[i]));
                    }
                }
                yield return null;
                Singleton<SceneMessagePanelUI>.Instance.SetLoadingText("Tweaks Aircraft | Loading:   " + aircraftTypeData.DisplayName, ((processedAircraftCount*100f)/(float)aircraftTypeList.Count).RoundToIntLikeANormalPerson().Clamp(5, 100));
                processedAircraftCount++;
            }

            aircraftGameObjectsSet.UnionWith(atc.aircraftPrefabs);
            aircraftGameObjectsSet.ExceptWith(AirportCEOAircraft.aircraftPrefabOverwrites.Keys);
            atc.aircraftPrefabs = aircraftGameObjectsSet.ToArray();
            List<AircraftModel> aircraftModelList = new List<AircraftModel>();
            foreach (GameObject prefab in atc.aircraftPrefabs)
            {
                aircraftModelList.Add(prefab.GetComponent<AircraftController>().am);
            }

            atc.GetType().GetField("aircraftModels",BindingFlags.NonPublic | BindingFlags.Instance).SetValue(atc, aircraftModelList.ToArray());

            working = false;
            //Singleton<SaveLoadGameDataController>.Instance.InitializeGameSession();
            yield break;
        }
        public IEnumerator packagedEnumerator(object original)
        {
            yield return base.StartCoroutine(Initilize());
            yield return original;
        }
        private GameObject MakeAircraftGameObject(AircraftTypeData aircraftTypeData, int index = 0)
        {

            GameObject copyOf = Singleton<AirTrafficController>.Instance.GetAircraftGameObject(aircraftTypeData.copyFrom);
            GameObject newGameObject;
            AircraftType aircraftType;

            if (aircraftTypeData.id[index] == aircraftTypeData.copyFrom)
            {
                //Debug.Log("ACEO Tweaks | Log: Aircraft Adder MakeAircraftGameObject Conditional True");
                //Instantiate
                newGameObject = GameObject.Instantiate(copyOf);

                //AircraftType
                if (!CustomEnums.TryGetAircraftType(aircraftTypeData.copyFrom, out aircraftType))
                {
                    Debug.LogError("ACEO Tweaks | Error: Couldn't find custom enum for " + copyOf.name);
                }
                aircraftType.size = aircraftTypeData.size;

                //name and transform of new GameObject
                newGameObject.name = aircraftType.id;
                newGameObject.transform.localEulerAngles = Vector3.zero;

                //Add the overwrite to dictionary
                if (AirportCEOAircraft.aircraftPrefabOverwrites.ContainsKey(copyOf))
                {
                    Debug.LogWarning("ACEO Tweaks | Warn: Duplicate overwrites for " + aircraftType.id);
                }
                else
                {
                    AirportCEOAircraft.aircraftPrefabOverwrites.Add(copyOf, newGameObject);
                }
            }
            else
            {
                //Debug.Log("ACEO Tweaks | Log: Aircraft Adder MakeAircraftGameObject Conditional Else");
                //Instantiate
                newGameObject = GameObject.Instantiate(copyOf);
                aircraftType = new AircraftType
                {
                    id = aircraftTypeData.id.Length > index ? aircraftTypeData.id[index] : aircraftTypeData.id[0],
                    size = aircraftTypeData.size
                };

                //name and transform of new GameObject
                newGameObject.name = aircraftType.id;
                newGameObject.transform.localEulerAngles = Vector3.zero;
                

                //Add the new AircraftType
                var method = typeof(CustomEnums).GetMethod("AddAircrafTypeRange", BindingFlags.Static | BindingFlags.NonPublic);
                if (method == null)
                {
                    Debug.LogError("ACEO Tweaks | ERROR: Couldn't find AddAircraftTypeRange method via reflection!");
                }
                method.Invoke(obj: null, parameters: new object[] { new AircraftType[] { aircraftType } } );
            
            }
            //Debug.Log("ACEO Tweaks | Log: Aircraft Adder MakeAircraftGameObject Conditional End");
            //Debug.Log("ACEO Tweaks | Log: Aircraft Adder is for "+aircraftType.id);
            //Debug.Log("ACEO Tweaks | Log: Aircraft Adder is for json " + aircraftTypeData.id[0] +" - "+ index);


            if (newGameObject == null)
            {
                Debug.LogError("ACEO Tweaks | Error: Aircraft Adder: newGameObject == null!");
            }
            AircraftController newAircraftController = newGameObject.GetComponent<AircraftController>();
            if (newAircraftController == null)
            {
                Debug.LogError("ACEO Tweaks | Error: Aircraft Adder: newAircraftController == null!");
            }
            AircraftModel newAircraftModel = newAircraftController.am;
            if (newAircraftModel == null)
            {
                Debug.LogError("ACEO Tweaks | Error: Aircraft Adder: newAircraftModel == null!");
            }

            //Debug.Log("ACEO Tweaks | Log: Aircraft Adder MakeAircraftGameObject Model Block Start");

            newAircraftModel.aircraftType = aircraftTypeData.id[index];  //must have an id at every index. Only manditory array.
            newAircraftModel.weightClass = aircraftTypeData.threeStepSize;
            newAircraftModel.manufacturer = aircraftTypeData.manufacturer.Length > index ? aircraftTypeData.manufacturer[index] : aircraftTypeData.manufacturer[0];
            newAircraftModel.modelNbr = aircraftTypeData.displayName.Length > index? aircraftTypeData.displayName[index] : aircraftTypeData.displayName[0];  
            newAircraftModel.maxPax = aircraftTypeData.capacity_PAX.Length > index ? aircraftTypeData.capacity_PAX[index] : aircraftTypeData.capacity_PAX[0];
            newAircraftModel.seatRows = aircraftTypeData.seatsAbreast.Length > index ? aircraftTypeData.seatsAbreast[index] : aircraftTypeData.seatsAbreast[0];

            //Debug.Log("ACEO Tweaks | Log: Aircraft Adder MakeAircraftGameObject Model Block End");

            short capULDLower = aircraftTypeData.capacityULDLowerDeck.Length > index ? aircraftTypeData.capacityULDLowerDeck[index] : aircraftTypeData.capacityULDLowerDeck[0];
            short capULDUpper = aircraftTypeData.capacityULDUpperDeck.Length > index ? aircraftTypeData.capacityULDUpperDeck[index] : aircraftTypeData.capacityULDUpperDeck[0];
            short conveyerPoints = aircraftTypeData.conveyerPoints.Length > index ? aircraftTypeData.conveyerPoints[index] : aircraftTypeData.conveyerPoints[0];
            newAircraftController.doNotUseULD = capULDLower + capULDUpper > 0 ? false : true;
            newAircraftController.requiresCargoTransferAssistance = (conveyerPoints > 0 || capULDLower + capULDUpper > 0) ? true : false; //belt or ULD

            //Debug.Log("ACEO Tweaks | Log: Aircraft Adder MakeAircraftGameObject Controller Block End");

            newAircraftModel.rangeKM = aircraftTypeData.range_KM.Length > index ? aircraftTypeData.range_KM[index] : aircraftTypeData.range_KM[0];
            newAircraftModel.flyingSpeed = aircraftTypeData.speed_KMH.Length > index ? aircraftTypeData.speed_KMH[index] : aircraftTypeData.speed_KMH[0];
            newAircraftModel.fuelTankCapacityLiters = aircraftTypeData.fuelCapacity_L.Length > index ? aircraftTypeData.fuelCapacity_L[index] : aircraftTypeData.fuelCapacity_L[0];

            string engineType = aircraftTypeData.engineType.Length > index ? aircraftTypeData.engineType[index] : aircraftTypeData.engineType[0]; //radial,inline,turboprop,turbojet,low_turbofan,turbofan,high_turbofan,afterburner
            switch (engineType)  //sets Prop/Jet; fuel type; afterburner
            {
                case "radial":
                case "piston":
                case "inline":
                    newAircraftModel.aircraftEngineType = Enums.AircraftEngineType.Prop;
                    newAircraftModel.fuelType = Enums.FuelType.Avgas100LL;
                    newAircraftController.hasAfterburner = false;
                    break;
                case "turboprop":
                    newAircraftModel.aircraftEngineType = Enums.AircraftEngineType.Prop;
                    newAircraftModel.fuelType = Enums.FuelType.JetA1;
                    newAircraftController.hasAfterburner = false;
                    break;
                case "turbojet":
                case "low_turbofan":
                case "turbofan":
                case "high_turbofan":
                    newAircraftModel.aircraftEngineType = Enums.AircraftEngineType.Jet;
                    newAircraftModel.fuelType = Enums.FuelType.JetA1;
                    newAircraftController.hasAfterburner = false;
                    break;
                case "afterburner":
                    newAircraftModel.aircraftEngineType = Enums.AircraftEngineType.Jet;
                    newAircraftModel.fuelType = Enums.FuelType.JetA1;
                    newAircraftController.hasAfterburner = true;
                    break;
            } //sets engine type, fuel type, and afterburner
            //Debug.Log("ACEO Tweaks | Log: Aircraft Adder MakeAircraftGameObject Engine Block End");

            newAircraftController.requiresElevatedAccess = aircraftTypeData.needStairs.Length > index ? aircraftTypeData.needStairs[index] : aircraftTypeData.needStairs[0];

            short cateringPoints = aircraftTypeData.cateringPoints.Length > index ? aircraftTypeData.cateringPoints[index] : aircraftTypeData.cateringPoints[0];
            newAircraftController.onlyUseOneCateringTruck = cateringPoints <=1 ? true : false;

            short jetbridgePoints = aircraftTypeData.jetbridgePoints.Length > index ? aircraftTypeData.jetbridgePoints[index] : aircraftTypeData.jetbridgePoints[0];
            newAircraftController.onlyUseOneJetway = jetbridgePoints <=1 ? true : false;

            //Debug.Log("ACEO Tweaks | Log: Aircraft Adder Make Aircraft Game Object Bottom");
            return newGameObject;
        }

        private void DoTweaksLiveryBakeIn(GameObject aircraftGameObject, AircraftTypeData aircraftTypeData)
        {
            GameObject perfCEOGameObject = GameObject.Find("PerformanceCEOActive");

            

            string filePath = aircraftTypeData.filePath.Replace("\\", "/");

            string[] jsonFiles = Directory.GetFiles(filePath, "*_Visual.json");
            string[] PNGfiles = Directory.GetFiles(filePath, "*.png");

            string[] specificJSONFiles = Directory.GetFiles(filePath, aircraftGameObject.name + "*_Visual.json");
            string[] specificPNGfiles = Directory.GetFiles(filePath, aircraftGameObject.name + "*.png");

            List<GameObject> componentGameObjects = new List<GameObject>();

            if (jsonFiles.Length == 0 || PNGfiles.Length == 0)
            {
                return;
            }

            if (specificJSONFiles.Length > 0)
            {
                jsonFiles[0] = specificJSONFiles[0];
            }

            if (specificPNGfiles.Length > 0)
            {
                PNGfiles[0] = specificPNGfiles[0];
            }

            GameObject tweaksContainer = new GameObject("tweaksContainer");

            LiveryData liveryData = Utils.CreateFromJSON<LiveryData>(Utils.ReadFile(jsonFiles[0]));
            byte[] data = File.ReadAllBytes(PNGfiles[0]);
            Texture2D texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(data);

            float downscaleAmount = GetDownscaleFloat();

            if (AirportCEOAircraftConfig.DownscaleLevel.Value != DownscaleEnums.DownscaleLevel.Original)
            {
                int newX = Utils.RoundToIntLikeANormalPerson((float)texture2D.width / downscaleAmount);
                int newY = Utils.RoundToIntLikeANormalPerson((float)texture2D.height / downscaleAmount);

                texture2D = DownscaleTexture(texture2D, newX, newY);
            }

            if (GameSettingManager.CompressImages)
            {
                texture2D.Compress(true);
            }
            LiveryComponent[] liveryComponetArray = liveryData.liveryComponent;
            Sprite[] spriteArray = new Sprite[liveryComponetArray.Length];
            Vector2 lhs = Vector2.zero;
            Vector2 lhs2 = Vector2.zero;

            for (int j = 0; j < liveryComponetArray.Length; j++)
            {
                LiveryComponent liveryComponent = liveryComponetArray[j];

                liveryComponent.slicePosition = RoundVecToInt(liveryComponent.slicePosition / downscaleAmount);
				liveryComponent.sliceSize = RoundVecToInt(liveryComponent.sliceSize / downscaleAmount);
				liveryComponent.scale *= downscaleAmount;

                liveryComponent.ClampValues(new Vector2((float)texture2D.width, (float)texture2D.height));
                if (lhs == Vector2.zero || lhs2 == Vector2.zero || lhs != liveryComponent.slicePosition || lhs2 != liveryComponent.sliceSize)
                {
                    Tweaks_PerformanceCEO.RAMReducer.Tweaks_RAMReducerManager.TweaksAircraftCall = true;
                    spriteArray[j] = Sprite.Create(texture2D, new Rect(liveryComponent.slicePosition.x, liveryComponent.slicePosition.y, liveryComponent.sliceSize.x, liveryComponent.sliceSize.y), liveryComponent.pivot, liveryData.pixelSize, 0U, SpriteMeshType.FullRect);
                    Tweaks_PerformanceCEO.RAMReducer.Tweaks_RAMReducerManager.TweaksAircraftCall = false;
                    lhs = liveryComponent.slicePosition;
                    lhs2 = liveryComponent.sliceSize;
                }
                else
                {
                    spriteArray[j] = spriteArray[j - 1];
                }
                GameObject newComponentGameObject = new GameObject(liveryComponent.name);
                newComponentGameObject.transform.SetParent(tweaksContainer.transform, false);
                newComponentGameObject.layer = LayerMask.NameToLayer("Aircraft");
                SpriteRenderer spriteRenderer = newComponentGameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteArray[j];
                spriteRenderer.sortingLayerName = "Aircraft";
                spriteRenderer.sortingOrder = liveryComponent.layerOrder;
                spriteRenderer.material = SingletonNonDestroy<DataPlaceholderMaterials>.Instance.generalDiffuseMaterial;
                spriteRenderer.flipX = ((int)liveryComponent.flip.x == 1);
                spriteRenderer.flipY = ((int)liveryComponent.flip.y == 1);
                newComponentGameObject.transform.localPosition = liveryComponent.position;
                newComponentGameObject.transform.eulerAngles = new Vector3(0f, 0f, liveryComponent.rotation);
                newComponentGameObject.transform.localScale = liveryComponent.scale;
                componentGameObjects.Add(newComponentGameObject);
            }

            tweaksContainer.transform.SetParent(aircraftGameObject.transform.Find("Sprite"), false);
            tweaksContainer.transform.localPosition = Vector3.zero;
            tweaksContainer.transform.localEulerAngles = Vector3.zero;

            LiveryActiveComponent lac = aircraftGameObject.GetComponent<LiveryActiveComponent>();
            if (lac == null)
            {
                lac = aircraftGameObject.AddComponent<LiveryActiveComponent>();
            }

            foreach (GameObject obj in componentGameObjects)
            {
                if (obj == null) { continue; }
                lac.DoLiveryComponentActions(obj);
            }


            


        }

        private static float GetDownscaleFloat()
        {
            float downscaleAmount = 1;
            switch (AirportCEOAircraftConfig.DownscaleLevel.Value)
            {
                case DownscaleEnums.DownscaleLevel.Original:
                    downscaleAmount = 1;
                    break;
                case DownscaleEnums.DownscaleLevel.Downscale2X:
                    downscaleAmount = 2;
                    break;
                case DownscaleEnums.DownscaleLevel.Downscale4X:
                    downscaleAmount = 4;
                    break;
                case DownscaleEnums.DownscaleLevel.Downscale8X:
                    downscaleAmount = 8; // Not recommended
                    break;
            };
            return downscaleAmount;
        }

        private static List<AircraftTypeData> ProccessAircraftPaths(string[] args)
        {
            List<AircraftTypeData> List = new List<AircraftTypeData>();
            List<AircraftTypeData> DirList = new List<AircraftTypeData>();

            foreach (string path in args)
            {
                if (File.Exists(path))
                {
                    // This path is a file

                    AircraftTypeData aircraftTypeData;
                    if (ProcessFile(path, out aircraftTypeData))
                    {
                        List.Add(aircraftTypeData);
                    }
                }
                else if (Directory.Exists(path))
                {
                    // This path is a directory

                    if (ProcessDirectory(path, out DirList))
                    {
                        List.AddRange(DirList);
                    }
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }
            }

            return List;
        }
        // Process all files in the directory passed in, recurse on any directories
        // that are found, and process the files they contain.
        private static bool ProcessDirectory(string targetDirectory, out List<AircraftTypeData> aircraftTypeDatas)
        {
            // Process the list of files found in the directory.
            List<AircraftTypeData> List = new List<AircraftTypeData>();
            List<AircraftTypeData> DirList = new List<AircraftTypeData>();
            AircraftTypeData aircraftTypeData;

            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                if(ProcessFile(fileName, out aircraftTypeData))
                {
                    List.Add(aircraftTypeData);
                }
            }

            // Recurse into subdirectories of this directory.

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                if(ProcessDirectory(subdirectory, out DirList))
                {
                    List.AddRange(DirList);
                }
            }


            aircraftTypeDatas = List;

            if (List.Count>0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // Insert logic for processing found files here.
        private static bool ProcessFile(string path, out AircraftTypeData aircraftTypeData)
        {
            if (Path.GetFileName(path).EndsWith("_Data.json"))
            {
                aircraftTypeData = Utils.CreateFromJSON<AircraftTypeData>(Utils.ReadFile(path));
                aircraftTypeData.filePath = Path.GetDirectoryName(path);
                return true;
            }
            else
            {
                aircraftTypeData = new AircraftTypeData();
                return false;
            }
        }

	    private static Vector2 RoundVecToInt(Vector2 vec)
	    {
		    return new Vector2(Utils.RoundToIntLikeANormalPerson(vec.x), Utils.RoundToIntLikeANormalPerson(vec.y));
	    }

	    private static Texture2D DownscaleTexture(Texture2D source, int newWidth, int newHeight)
	    {
		    RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);

		    RenderTexture.active = rt;

		    Graphics.Blit(source, rt);
		    source.Resize(newWidth, newHeight, TextureFormat.ARGB32, false); //.ARGB32
		    source.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0,0);
		    source.Apply();
		    RenderTexture.active = null;
		    RenderTexture.ReleaseTemporary(rt);
		    return source;
	    }


    }
}
