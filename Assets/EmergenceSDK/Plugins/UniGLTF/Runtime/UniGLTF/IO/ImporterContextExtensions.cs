using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VRMShaders;

namespace UniGLTF
{
    public static class ImporterContextExtensions
    {
        /// <summary>
        /// Build unity objects from parsed gltf
        /// </summary>
        public static RuntimeGltfInstance Load(this ImporterContext self)
        {
            var meassureTime = new ImporterContextSpeedLog();
            var immediateCaller = new ImmediateCaller();
            var task = self.LoadAsync(immediateCaller, meassureTime.MeasureTime);
            if (!task.GetAwaiter().IsCompleted)
            {
                throw new Exception();
            }
            
            if (task.Status.IsFaulted())
            {
                throw new AggregateException(immediateCaller.Exception);
            }

            if (Symbols.VRM_DEVELOP)
            {
                Debug.Log($"{self.Data.TargetPath}: {meassureTime.GetSpeedLog()}");
            }

            return task.GetAwaiter().GetResult();
        }
    }
}
