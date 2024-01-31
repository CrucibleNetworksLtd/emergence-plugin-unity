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
            var task = self.LoadAsync(new ImmediateCaller(), meassureTime.MeasureTime);
            if (!task.GetAwaiter().IsCompleted)
            {
                throw new Exception();
            }
            if (task.Status.IsFaulted())
            {
                try
                {
                    task.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    throw new AggregateException(e);
                }
            }

            if (Symbols.VRM_DEVELOP)
            {
                Debug.Log($"{self.Data.TargetPath}: {meassureTime.GetSpeedLog()}");
            }

            return task.GetAwaiter().GetResult();
        }
    }
}
