using System;
using System.Collections.Generic;
using System.IO;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;
using TRModelTransporter.Utilities;

namespace TRModelTests
{
    class Program
    {
        static void Main()
        {
            string lvl = LevelNames.GW;
            string sourceDir = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\TRGE\Edits\8d8fa943d444d925c78911433387c2e2\Backup");
            string targetDir = Environment.ExpandEnvironmentVariables(@"%PROGRAMFILES(X86)%\Steam\steamapps\common\Tomb Raider (II)\data");

            Dictionary<TR2Entities, TR2Entities> entitySwaps = new Dictionary<TR2Entities, TR2Entities>
            {
                [TR2Entities.Lara] = TR2Entities.LaraHome,
                [TR2Entities.TigerOrSnowLeopard] = TR2Entities.Winston,
                [TR2Entities.Spider] = TR2Entities.Rat,
                [TR2Entities.TRex] = TR2Entities.XianGuardSpear,
                [TR2Entities.Crow] = TR2Entities.Eagle
            };

            /////////////
            string sourceLevelPath = Path.Combine(sourceDir, lvl);
            string targetLevelPath = Path.Combine(targetDir, lvl);
            string deduplicationPath = @"Resources\Textures\Deduplication\" + lvl + "-TextureRemap.json";

            TR2LevelReader reader = new TR2LevelReader();
            TR2Level level = reader.ReadLevel(sourceLevelPath);

            TRLevelTextureDeduplicator deduplicator = new TRLevelTextureDeduplicator { Level = level };
            deduplicator.Deduplicate(deduplicationPath);
                        
            TRModelImporter importer = new TRModelImporter
            {
                Level = level,
                LevelName = lvl,
                EntitiesToImport = entitySwaps.Values,
                EntitiesToRemove = entitySwaps.Keys,
                ClearUnusedSprites = true,
                TextureRemapPath = deduplicationPath
            };

            importer.Import();

            foreach (TR2Entity entity in level.Entities)
            {
                TR2Entities e = (TR2Entities)entity.TypeID;
                if (entitySwaps.ContainsKey(e))
                {
                    entity.TypeID = (short)TR2EntityUtilities.TranslateEntityAlias(entitySwaps[e]);
                }
            }

            TR2LevelWriter writer = new TR2LevelWriter();
            writer.WriteLevelToFile(level, targetLevelPath);
        }
    }
}