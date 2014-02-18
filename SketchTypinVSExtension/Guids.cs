// Guids.cs
// MUST match guids.h
using System;

namespace Company.SketchTypinVSExtension
{
    static class GuidList
    {
        public const string guidSketchTypinVSExtensionPkgString = "f755a37d-194b-4066-98ff-634093eb726d";
        public const string guidSketchTypinVSExtensionCmdSetString = "5b3dbf65-19d1-4fad-a52a-6de170c1ca45";
        public const string guidToolWindowPersistanceString = "2dff4c38-4d6c-4315-8626-a361f3f5980c";
        public const string guidToolWindowPersistanceString1 = "2dff4c38-4d6c-4315-8626-a361f3f5981c";

        public static readonly Guid guidSketchTypinVSExtensionCmdSet = new Guid(guidSketchTypinVSExtensionCmdSetString);
    };
}