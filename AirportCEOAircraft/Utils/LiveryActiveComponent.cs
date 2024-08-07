﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft;
using AirportCEOFlightLimitTweak;
using AirportCEOTweaksCore;

namespace AirportCEOAircraft
{
    class LiveryActiveComponent : MonoBehaviour
    {

        bool LiveyComponentParse(string name, out HashSet<GameObject> gameObjects, out HashSet<string> verbs, out string[] parameters)
        {
            bool exactly = false;

            HashSet<string> groupWords = new HashSet<string>() ;
            HashSet<string> exactStrings = new HashSet<string>();

            verbs = new HashSet<string>();
            gameObjects = new HashSet<GameObject>();
            parameters = new string[] { };


            List<string> nounsList = Singleton<ModsController>.Instance.LiveryGroupWords();
            List<string> verbsList = Singleton<ModsController>.Instance.LiveryActionWords();

            if (name == null || !name.ToLower().Contains("config")) { return false; }

            string[] sections;
            sections = name.ToLower().Split('_');

            int n = 0;
            for (int i = 0; i < sections.Length; i++)
            {

                if (sections[i] == "config" || sections[i] == "" || sections[i] == "_") { continue; }

                // ------------------------------EXACTLY--------------------------------------------

                if (sections[i] == "exactly" || sections[i] == "xact")
                {
                    exactly = (!exactly);
                    if (AirportCEOAircraftConfig.LiveryLogs.Value)
                    {
                        Debug.LogError("ACEO Tweaks | Livery Debug: Found xact");
                    }
                    continue;
                }

                if (exactly)
                {
                    exactStrings.Add(sections[i]);
                    if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogError("ACEO Tweaks | Livery Debug: nouns += " + sections[i]); }
                    continue;
                }
                
                    // -------------------------------GROUPS---------------------------------------------

                if (verbs.Count == 0) //only look for groups if we aren't to a verb yet
                {
                    n = groupWords.Count;
                    foreach (string word in nounsList) // check for all group words
                    {
                        if (sections[i] == word)
                        {
                            groupWords.Add(word);
                            if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogError("ACEO Tweaks | Livery Debug: groupWord+=" + word); }
                            break;
                        }
                    }
                    if (groupWords.Count == n + 1) { continue; } //if we added a group word no need to keep looking
                }

                if (groupWords.Count == 0 && exactStrings.Count == 0) { continue; } //Need at least one noun before verbs


                    // ---------------------------------VERBS---------------------------------------------


                n = verbs.Count;
                foreach (string word in verbsList)
                {
                    if (sections[i] == word)
                    {
                        verbs.Add(word);
                        if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogError("ACEO Tweaks | Livery Debug: verbs+=" + word); }
                        break;
                    }
                } //check for all action words
                if (verbs.Count == n + 1) { continue; } //if we added a group word no need to keep looking

                if (verbs.Count == 0) { continue; } //Need at least one verb for parameters

                // ---------------------------------PARAMETERS-----------------------------------------

                //Debug.LogError("Reached PARAMETERS");
                parameters.Union(new string[] { sections[i] });
                if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogError("ACEO Tweaks | Livery Debug: parameters+=" + sections[i]); }


            }

            if (groupWords.Contains("self"))
            {
                exactStrings.Add(name);
                groupWords.Remove("self");
            }

            if (verbs.Count == 0 || (exactStrings.Count == 0 && groupWords.Count == 0))
            { Debug.LogWarning("ACEO Tweaks | WARN: LiveryComponetParse is returning false for insufficient definition on "+name + " verbs:"+verbs.Count.ToString()+" nouns:" + exactStrings.Count.ToString()+" groups:" + groupWords.Count.ToString()); return false; }




            try
            {
                
                gameObjects = GameObjectsFromStrings(groupWords.ToArray<string>(), exactStrings.ToArray<string>());
                
            }
            catch
            {
                Debug.LogError("Airport CEO Tweaks | Error: Couldn't GameObjectsFromStrings in Livery Active Component!");
                return false;
            }
            return true;

        }

        public bool DoLiveryComponentActions(GameObject originalComponent)
        {
            string name = originalComponent.name;
            bool flag = false;
            bool flag2;
            HashSet<String> verbs;
            HashSet<GameObject> gameObjects;
            string[] parameters;

            float z = originalComponent.transform.localPosition.z;
            //z = (z <= -99f) ? 0f : z;
            if (z <= -99)
            {
                if (AirportCEOAircraftConfig.LiveryLogs.Value)
                    { 
                    Debug.LogError("ACEO Tweaks | Livery Debug: found "+name+"at z="+z.ToString()+" : moving to z=0.");
                    }

                Vector3 newPos = new Vector3(originalComponent.transform.localPosition.x, originalComponent.transform.localPosition.y, 0.010f);
                originalComponent.transform.localPosition = newPos;
                
                if (AirportCEOAircraftConfig.LiveryLogs.Value)
                { 
                    Debug.LogError("ACEO Tweaks | Livery Debug: " + name + " now at local z=" + originalComponent.transform.localPosition.z.ToString("0.0000000"));
                    Debug.LogError("ACEO Tweaks | Livery Debug: " + name + " now at global z=" + originalComponent.transform.position.z.ToString("0.0000000"));
                }
            }
            else
            {
                if (AirportCEOAircraftConfig.LiveryLogs.Value)
                { 
                    //Debug.LogError("ACEO Tweaks | Livery Debug: found " + name + "at z=" + z.ToString() + " : not moving.");
                }
            }
            try
            {
                flag2 = LiveyComponentParse(name, out HashSet<GameObject> gO, out HashSet<string> v, out string[] p);
                gameObjects = gO;
                verbs = v;
                parameters = p;
            }

            catch { Debug.LogError("ACEO Tweaks | ERROR: could not parse livery component " + name); return false; } //fetch the livery in try/catch

            if (!flag2) { return false; }

            foreach (string verb in verbs)
            {
                switch (verb)
                {
                    case "setpax":    if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogWarning("ACEO Tweaks | WARN: attempted to verb " + verb + ". not implimented!"); } break;
                    case "setrows":   if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogWarning("ACEO Tweaks | WARN: attempted to verb " + verb + ". not implimented!"); } break;
                    case "setstairs": if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogWarning("ACEO Tweaks | WARN: attempted to verb " + verb + ". not implimented!"); } break;
                    case "settitle":  if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogWarning("ACEO Tweaks | WARN: attempted to verb " + verb + ". not implimented!"); } break;

                    case "moveabs": Move(gameObjects, originalComponent.transform.localPosition, false, originalComponent.transform.localEulerAngles.z); flag = true; break;
                    case "moverel": Move(gameObjects, originalComponent.transform.localPosition, true, originalComponent.transform.localEulerAngles.z); flag = true; break;
                    case "enable": EnableDisable(gameObjects, true); flag = true; break;
                    case "disable": EnableDisable(gameObjects, false); flag = true; break;
                    case "makeshadow": ReplaceShadowSpriteWith(gameObjects); EnableDisable(gameObjects,false); break;
                    case "setlayerorder": SetLayerOrderParse(gameObjects, parameters); break;
                    case "makewindow": MakeChildOf(gameObjects, new string[] { "nightwindows" }); SetMaterial(gameObjects, "nonlit"); break;
                    case "makenonlit": SetMaterial(gameObjects, "nonlit"); break;
                    case "makelightsource": if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogWarning("ACEO Tweaks | WARN: attempted to verb " + verb + ". not implimented!"); } break;
                    case "makelightsprite": if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogWarning("ACEO Tweaks | WARN: attempted to verb " + verb + ". not implimented!"); } break;
                    case "makechildof": MakeChildOf(gameObjects, parameters); break;

                    default:
                        Debug.LogError("ACEO Tweaks | ERROR: attempted to verb " + verb + ". Not recognised!");
                        break;
                }
            }

            return flag;
        }

        // Group words implimneted below!
        HashSet<GameObject> GameObjectsFromStrings(string[] groupWords, string[] nouns)
        {
            HashSet<GameObject> gameObjects = new HashSet<GameObject>();
            
            gameObjects.UnionWith(Exact(nouns));

            foreach (string word in groupWords)
            {
                HashSet<GameObject> union = new HashSet<GameObject>();
                switch (word)
                {
                    case "wings"            :           union = EasySearch("wing"); union.UnionWith(EasyExact("Flaps", true)); union.IntersectWith(EasyExact("Sprite",true)) ; break;

                    case "tail"             :           union = EasySearch("tail"); union.IntersectWith(EasyExact("Sprite", true)); break;

                    case "shadow"           :           union = EasyExact("Shadow");  break;

                    case "lights"           :           union = EasyExact("Lights",true); break;

                    case "effects"          :           union = EasyExact("Effects", true); break;

                    case "flaps"            :           union = EasyExact("Flaps", true);  break;

                    case "windows"          :           union = EasyExact("NightWindows", true); break; 

                    case "frontdoors"       :           union = EasyExact("Positions", true); union.IntersectWith(EasySearch("door")); union.IntersectWith(EasySearch("front")) ; break;

                    case "reardoors"        :           union = EasyExact("Positions", true); union.IntersectWith(EasySearch("door")); union.IntersectWith(EasySearch("rear")); break;

                    case "towbar"           :           union = EasyExact("TowBarPoint"); union.UnionWith(EasyExact("TurnPosition")); break;

                    case "audio"            :           union = EasyExact("Audio", true);  break;

                    case "groundequipment"  :           union = EasyExact("ToggleableSprites",true) ;  break;

                    case "livery"           :           union = EasyExact("Livery", true); break;



                    case "aircraftconfig"   :           Debug.LogError("ACEO Tweaks | WARN: Tried to use " + word + ". This feature is not yet implimented!");  break;



                    case "exactly"          :           Debug.LogError("ACEO Tweaks | ERROR: Tried to use groupWord " + word + ". This word should not appear in this words list!"); break;
                    case "xact"             :           Debug.LogError("ACEO Tweaks | ERROR: Tried to use groupWord " + word + ". This word should not appear in this words list!"); break;
                    case "self"             :           Debug.LogError("ACEO Tweaks | ERROR: Tried to use groupWord " + word + ". This word should not appear in this words list!"); break;
                    default                 :           Debug.LogError("ACEO Tweaks | ERROR: Tried to use groupWord " + word + ". This word is not recognised!"); break;
                        
                }
                gameObjects.UnionWith(union);
            }


            HashSet<GameObject> EasySearch(string term, bool getChildren = false)
            {
                return Search(new string[] { term }, getChildren);
            }
            HashSet<GameObject> Search(string[] terms, bool getChildren = false)
            {
                return SearchUnder(terms,this.gameObject,getChildren);
            }
            HashSet<GameObject> SearchUnder(string[] terms, GameObject parent, bool getChildren = false)
            {
                HashSet<GameObject> searched = new HashSet<GameObject>();

                foreach (string term in terms)
                {
                    Transform[] children = parent.transform.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in children)
                    {
                        if (child.gameObject.name.ToLower().Contains(term.ToLower()))
                        {
                            searched.Add(child.gameObject);
                            if (getChildren)
                            {
                                Transform[] grandchildren = child.transform.GetComponentsInChildren<Transform>(true);
                                foreach (Transform t in grandchildren)
                                {
                                    searched.Add(t.gameObject);
                                }
                                searched.Remove(child.gameObject);
                            }
                        }
                    }
                }
                return searched;
            }

            HashSet<GameObject> EasyExact(string term, bool getChildren = false)
            {
                return Exact(new string[] { term }, getChildren);
            }
            HashSet<GameObject> Exact(string[] terms, bool getChildren = false)
            {
                return ExactUnder(terms, this.gameObject, getChildren);
            }
            HashSet<GameObject> ExactUnder(string[] terms, GameObject parent, bool getChildren = false)
            {
                HashSet<GameObject> matched = new HashSet<GameObject>();

                foreach (string term in terms)
                {
                    Transform[] children = parent.transform.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in children)
                    {
                        if (child.gameObject.name.ToLower()==term.ToLower())
                        {
                            matched.Add(child.gameObject);
                            if (getChildren)
                            {
                                Transform[] grandchildren = child.transform.GetComponentsInChildren<Transform>(true);
                                foreach (Transform t in grandchildren)
                                {
                                    matched.Add(t.gameObject);
                                }
                                matched.Remove(child.gameObject);
                            }
                        }
                    }
                }
                return matched;
            }

            return gameObjects;
        }

        Material MaterialFromString(string matString, out int layer)
        {
            layer = 10;
            switch (matString.ToLower())
            {
                case "normal": return SingletonNonDestroy<DataPlaceholderMaterials>.Instance.generalDiffuseMaterial;

                case "window":
                case "non-lit":
                case "nonlit": layer = 9;  return SingletonNonDestroy<DataPlaceholderMaterials>.Instance.nonLitMateral;

                case "heathaze": layer = 13;  return SingletonNonDestroy<DataPlaceholderMaterials>.Instance.heatHaze;

                case "externalspritesmaterial": return SingletonNonDestroy<DataPlaceholderMaterials>.Instance.externalSpritesMaterial;

                default: Debug.LogError("ACEO Tweaks | WARN: Material " + matString + " not recognised: returned default"); return SingletonNonDestroy<DataPlaceholderMaterials>.Instance.generalDiffuseMaterial;
            }
        }

        void Move(HashSet<GameObject> gameObjects, Vector3 position, bool rel, float rotation = 0f)
        {
            float scale = Scale();
            foreach (GameObject obj in gameObjects)
            {
                if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogError("ACEO Tweaks | Livery Debug: moving " + obj.name + " oldpos = " + obj.transform.localPosition.ToString()); }
                if (rel)
                {
                    obj.transform.position += (position*scale);
                }
                else
                {
                    obj.transform.localPosition = (position*scale);
                }
                if (AirportCEOAircraftConfig.LiveryLogs.Value) { Debug.LogError("ACEO Tweaks | Livery Debug: moved " + obj.name + " newpos = " + obj.transform.localPosition.ToString() + "(move scaled by " + scale + ")"); }
                obj.transform.Rotate(Vector3.forward, rotation);
            }
        }
        void EnableDisable(HashSet<GameObject> gameObjects, bool flag)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (AirportCEOAircraftConfig.LiveryLogs.Value)
                {
                    Debug.LogError("ACEO Tweaks | Livery Debug: enable/disabled " + obj.name + " enabled = " + flag.ToString());
                }
                Utils.AttemptEnableDisableGameObject(obj, flag);
            }
        }
        void SetMaterial(HashSet<GameObject> gameObjects, string matString)
        {
            Material m = MaterialFromString(matString, out int layer);
            foreach (GameObject obj in gameObjects)
            {
                SpriteRenderer r = obj.GetComponent<SpriteRenderer>();
                r.material = m;
                if (AirportCEOAircraftConfig.LiveryLogs.Value)
                {
                    Debug.LogError("ACEO Tweaks | Livery Debug: changed " + obj.name + "material to " + matString);
                }
            }
            SetLayerType(gameObjects, layer);
        }
        void SetLayerOrder(HashSet<GameObject> gameObjects, int layer)
        {
            foreach (GameObject obj in gameObjects)
            {
                
                    SpriteRenderer r = obj.GetComponent<SpriteRenderer>();
                    r.sortingOrder = layer;
                    if (AirportCEOAircraftConfig.LiveryLogs.Value)
                    {
                    Debug.LogError("ACEO Tweaks | Livery Debug: changed " + obj.name + "layer order to "+layer.ToString());
                    }

            }
        }
        void SetLayerOrderParse(HashSet<GameObject> gameObjects, string[] parameters)
        {
            int order = 569228;
            foreach (string param in parameters)
            {
                try { order = Int32.Parse(param); }
                catch { continue; }
                if (order > -20 && order < 20)
                { break; }
            }
            if (order == 569228)
            {
                order = 1;
                Debug.LogError("ACEO Tweaks | Warn: Could not parse order parameter as int; reverted to 1.");
            }
            SetLayerOrder(gameObjects, order);
        }
        void SetLayerType(HashSet<GameObject> gameObjects, int layer)
        {
            foreach (GameObject obj in gameObjects)
            {
                obj.layer = layer;
                if (AirportCEOAircraftConfig.LiveryLogs.Value)
                {
                    Debug.LogError("ACEO Tweaks | Livery Debug: changed " + obj.name + "layer type");
                }
            }
        }
        void MakeChildOf(HashSet<GameObject> gameObjects, string[] parameters)
        {
            GameObject parent = GameObjectsFromStrings(new string[] { }, parameters).Last();
            if (parent == null) { Debug.LogError("AirportCEO Tweaks | WARN: did not find gameobject in parameters!"); return; }

            foreach (GameObject obj in gameObjects)
            {
                obj.transform.SetParent(parent.transform);
                if (AirportCEOAircraftConfig.LiveryLogs.Value)
                {
                    Debug.LogError("ACEO Tweaks | Livery Debug: changed " + obj.name + " parent/group to "+parent.name);
                }
            }
        }
        void ReplaceShadowSpriteWith(HashSet<GameObject> gameObjects)
        {
            GameObject gameObject = gameObjects.Last();
            GameObject shadowObject = null;

            Transform[] children = this.gameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.gameObject.name.ToLower() == "shadow")
                {
                    shadowObject = child.gameObject;
                }
            }

            if (shadowObject == null)
            {
                Debug.LogError("ACEO Tweaks | ERROR: Failed to find vanilla shadow object!");
                return;
            }

            shadowObject.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
            shadowObject.transform.Rotate(Vector3.forward,gameObject.transform.localEulerAngles.z);
            shadowObject.transform.localScale = Vector3.Scale(shadowObject.transform.localScale, gameObject.transform.localScale);


            ShadowHandler handler = shadowObject.GetComponent<ShadowHandler>();

            if (handler!=null)
            {
                handler.referenceTransform = gameObject.transform;
                handler.cachedReferenceTransformPosition = gameObject.transform.position;
                handler.cachedReferenceTransformScale = shadowObject.transform.localScale;
                shadowObject.transform.position = gameObject.transform.position;
                //handler.UpdateShadow();
            }

            SpriteRenderer oldRender  = gameObject.GetComponent<SpriteRenderer>();

            if (oldRender !=null)
            {
                oldRender.forceRenderingOff = true;
            }
        }
        float Scale()
        {
            float scale = 1f;
            AircraftScaleManager scaleManager = gameObject.GetComponent<AircraftScaleManager>();
            if (scaleManager != null)
            {
                scale = scaleManager.scale;
            }
            else
            {
                //Debug.LogWarning("ACEO Tweaks | Warn: Livery Active Component couldn't find aircraft scale manager");
            }
            return 1f;
            //return scale;
        }
    }
}
