// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2019 Marcel Koester
//                                www.ilgpu.net
//
// File: CudaNativeMethods.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime.Cuda;
using System;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Naming Styles

namespace ILGPU.SharpDX.Cuda
{
    enum CudaGraphicsMapFlags
    {
        None = 0,
        ReadOnly = 1,
        WriteDiscard = 2
    }

    enum CudaGraphicsRegisterFlags
    {
        None = 0,
        ReadOnly = 1,
        WriteDiscard = 2,
        SurfaceLoadStore = 4,
        TextureGather = 8
    }

    enum CudaArrayFormat
    {
        None = 0,

        UInt8 = 1,
        UInt16 = 2,
        UInt32 = 3,

        Int8 = 8,
        Int16 = 9,
        Int32 = 10,

        F16 = 16,
        F32 = 32
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CudaArrayDescriptor
    {
        public IntPtr width;
        public IntPtr height;
        public CudaArrayFormat arrayFormat;
        public int numChannels;
    }

    enum CudaMemoryType
    {
        Host = 1,
        Device = 2,
        Array = 3,
        Unified = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    struct CudaMemcpy2DArgs
    {
        public IntPtr srcXInBytes;
        public IntPtr srcY;

        public CudaMemoryType srcMemoryType;
        public IntPtr srcHost;
        public IntPtr srcDevice;
        public IntPtr srcArray;
        public IntPtr srcPitch;

        public IntPtr dstXInBytes;
        public IntPtr dstY;

        public CudaMemoryType dstMemoryType;
        public IntPtr dstHost;
        public IntPtr dstDevice;
        public IntPtr dstArray;
        public IntPtr dstPitch;

        public IntPtr WidthInBytes;
        public IntPtr Height;
    }

    static unsafe class CudaNativeMethods
    {
        internal const string CudaDriverLibName = "nvcuda";

        [DllImport(CudaDriverLibName)]
        public static extern CudaError cuGraphicsD3D11RegisterResource(
            [Out] out IntPtr cudaGraphicsResource,
            [In] IntPtr d3dResource,
            [In] CudaGraphicsRegisterFlags flags);

        [DllImport(CudaDriverLibName)]
        public static extern CudaError cuGraphicsUnregisterResource(
            [In] IntPtr cudaGraphicsResource);

        [DllImport(CudaDriverLibName)]
        public static extern CudaError cuGraphicsResourceSetMapFlags(
            [In] IntPtr cudaGraphicsResource,
            [In] CudaGraphicsMapFlags flags);

        [DllImport(CudaDriverLibName)]
        public static extern CudaError cuGraphicsMapResources(
            [In] int count,
            [In] IntPtr* cudaGraphicsResources,
            [In] IntPtr cudaStream);

        [DllImport(CudaDriverLibName)]
        public static extern CudaError cuGraphicsUnmapResources(
            [In] int count,
            [In] IntPtr* cudaGraphicsResources,
            [In] IntPtr cudaStream);

        [DllImport(CudaDriverLibName, EntryPoint = "cuGraphicsResourceGetMappedPointer_v2")]
        public static extern CudaError cuGraphicsResourceGetMappedPointer(
            [Out] out IntPtr dataPtr,
            [Out] out UIntPtr size,
            [In] IntPtr cudaGraphicsResource);

        [DllImport(CudaDriverLibName)]
        public static extern CudaError cuGraphicsSubResourceGetMappedArray(
            [Out] out IntPtr cudaArray,
            [In] IntPtr cudaGraphicsResource,
            [In] uint arrayIndex,
            [In] uint mipLevel);

        [DllImport(CudaDriverLibName, EntryPoint = "cuArrayGetDescriptor_v2")]
        public static extern CudaError cuArrayGetDescriptor(
            [Out] out CudaArrayDescriptor descriptor,
            [In] IntPtr cudaArray);

        [DllImport(CudaDriverLibName, EntryPoint = "cuMemcpy2D_v2")]
        public static extern CudaError cuMemcpy2D(
            ref CudaMemcpy2DArgs args);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Cuda-array")]
        public static int GetByteSize(CudaArrayFormat format)
        {
            switch (format)
            {
                case CudaArrayFormat.Int8:
                case CudaArrayFormat.UInt8:
                    return 1;
                case CudaArrayFormat.Int16:
                case CudaArrayFormat.UInt16:
                case CudaArrayFormat.F16:
                    return 2;
                case CudaArrayFormat.Int32:
                case CudaArrayFormat.UInt32:
                case CudaArrayFormat.F32:
                    return 4;
                default:
                    throw new NotSupportedException("Not supported Cuda-array format");
            }
        }
    }
}

#pragma warning restore IDE1006 // Naming Styles
