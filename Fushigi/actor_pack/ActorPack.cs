﻿using Fushigi.actor_pack.components;
using Fushigi.Byml.Serializer;
using Fushigi.gl.Bfres;
using Fushigi.SARC;
using Fushigi.util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fushigi
{
    public class ActorPackCache
    {
        public static Dictionary<string, ActorPack> Actors = new Dictionary<string, ActorPack>();

        public static ActorPack Load(string gyml)
        {
            string path = FileUtil.FindContentPath(Path.Combine("Pack", "Actor", $"{gyml}.pack.zs"));
            if (!File.Exists(path))
                return null;

            if (!Actors.ContainsKey(gyml))
                Actors.Add(gyml, new ActorPack(path));

            return Actors[gyml];
        }
    }

    public class ActorPack
    {
        public ModelInfo DrawArrayModelInfoRef;
        public ModelInfo ModelInfoRef;

        public string Category = "";

        public ActorPack(string path)
        {
            byte[] fileBytes = FileUtil.DecompressFile(path);
            SARC.SARC sarc = new SARC.SARC(new MemoryStream(fileBytes));

            //Notes:
            //We load the component list rather than folders as there can be multiple components of the same type
            //Model info for example has multiple from model skin to use varied bfres skins
            foreach (var file in sarc.GetFiles("Actor"))
            {
                var paramInfo = BymlSerialize.Deserialize<ActorParam>(sarc.OpenFile(file));

                LoadComponents(sarc, paramInfo);

                if (!string.IsNullOrEmpty(paramInfo.Category))
                    this.Category = paramInfo.Category;
            }
        }

        private void LoadComponents(SARC.SARC sarc, ActorParam param)
        {
            if (param.Components == null)
                return;

            foreach (var component in param.Components)
            {
                string filePath = GetPathGyml((string)component.Value);
                var data = sarc.OpenFile(filePath);

                //Check if the component is present in the pack file.
                if (data == null)
                    continue;

                switch (component.Key)
                {
                    case "DrawArrayModelInfoRef":
                        this.DrawArrayModelInfoRef = BymlSerialize.Deserialize<ModelInfo>(data);
                        Console.WriteLine($"DrawArrayModelInfoRef {DrawArrayModelInfoRef.mModelName}");
                        break;
                    case "ModelInfoRef":
                        this.ModelInfoRef = BymlSerialize.Deserialize<ModelInfo>(data);
                        Console.WriteLine($"ModelInfoRef {ModelInfoRef.mModelName}");
                        break;
                }
            }
        }

        private string GetPathGyml(string path)
        {
            string gyml = path.Replace("Work/", string.Empty);
            return gyml.Replace(".gyml", ".bgyml");
        }

        class ActorParam
        {
            public string Category { get; set; }
            public Dictionary<string, object> Components { get; set; } 
        }
    }
}