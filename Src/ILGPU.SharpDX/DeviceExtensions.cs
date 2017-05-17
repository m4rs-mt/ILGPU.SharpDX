// -----------------------------------------------------------------------------
//                               ILGPU.SharpDX
//                     Copyright (c) 2016-2017 Marcel Koester
//                                www.ilgpu.net
//
// File: DeviceExtensions.cs
//
// This file is part of ILGPU and is distributed under the University of
// Illinois Open Source License. See LICENSE.txt for details.
// -----------------------------------------------------------------------------

using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ILGPU.SharpDX
{
    /// <summary>
    /// ILGPU-specific extensions for DX devices.
    /// </summary>
    public static class DeviceExtensions
    {
        /// <summary>
        /// Creates an associated accelerator of the given type.
        /// </summary>
        /// <param name="device">The source DX device</param>
        /// <param name="context">The ILGPU context DX device</param>
        /// <param name="type">The type of the accelerator to create.</param>
        /// <returns>The created accelerator.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We can only realize an extension to a device object")]
        public static Accelerator CreateAssociatedAccelerator(
            this Device device,
            Context context,
            AcceleratorType type)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            switch (type)
            {
                case AcceleratorType.CPU:
                    return new CPUAccelerator(context);
                case AcceleratorType.Cuda:
                    return new CudaAccelerator(
                        context,
                        device.NativePointer,
                        CudaAcceleratorFlags.ScheduleAuto);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// Creates an associated accelerator of the given type.
        /// </summary>
        /// <param name="device">The source DX device</param>
        /// <param name="context">The ILGPU context DX device</param>
        /// <param name="numThreads">The number of CPU threads.</param>
        /// <returns>The created accelerator.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "We can only realize an extension to a device object")]
        public static Accelerator CreateAssociatedCPUAccelerator(
            this Device device,
            Context context,
            int numThreads)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return new CPUAccelerator(context, numThreads);
        }
    }
}
