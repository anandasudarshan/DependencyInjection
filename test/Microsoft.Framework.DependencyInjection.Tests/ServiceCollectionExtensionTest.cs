// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.DependencyInjection.Tests.Fakes;
using Xunit;

namespace Microsoft.Framework.DependencyInjection
{
    public class ServiceCollectionExtensionTest
    {
        private static readonly FakeService _instance = new FakeService();

        public static TheoryData AddImplementationTypeData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeService);
                return new TheoryData<Action<IServiceCollection>, Type, Type, ServiceLifetime>
                {
                    { collection => collection.AddTransient(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient<IFakeService>(), serviceType, serviceType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient(implementationType), implementationType, implementationType, ServiceLifetime.Transient },

                    { collection => collection.AddScoped(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped<IFakeService>(), serviceType, serviceType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped(implementationType), implementationType, implementationType, ServiceLifetime.Scoped },

                    { collection => collection.AddSingleton(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton<IFakeService>(), serviceType, serviceType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton(implementationType), implementationType, implementationType, ServiceLifetime.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddImplementationTypeData))]
        public void AddWithTypeAddsServiceWithRightLifecyle(Action<IServiceCollection> addTypeAction,
                                                            Type expectedServiceType,
                                                            Type expectedImplementationType,
                                                            ServiceLifetime lifeCycle)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addTypeAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            Assert.Equal(expectedServiceType, descriptor.ServiceType);
            Assert.Equal(expectedImplementationType, descriptor.ImplementationType);
            Assert.Equal(lifeCycle, descriptor.Lifetime);
        }

        public static TheoryData AddImplementationFactoryData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var objectType = typeof(object);
                var implementationType = typeof(FakeService);

                return new TheoryData<Action<IServiceCollection>, Type, Type, ServiceLifetime>
                {
                    { collection => collection.AddTransient(serviceType, s => new FakeService()),serviceType, objectType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient<IFakeService>(s => new FakeService()),serviceType, serviceType, ServiceLifetime.Transient },                    
                    { collection => collection.AddTransient<IFakeService, FakeService>(s => new FakeService()),serviceType, implementationType, ServiceLifetime.Transient },

                    { collection => collection.AddScoped(serviceType, s => new FakeService()),serviceType, objectType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped<IFakeService>(s => new FakeService()),serviceType, serviceType, ServiceLifetime.Scoped },                    
                    { collection => collection.AddScoped<IFakeService, FakeService>(s => new FakeService()),serviceType, implementationType, ServiceLifetime.Scoped },

                    { collection => collection.AddSingleton(serviceType, s => new FakeService()),serviceType, objectType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton<IFakeService>(s => new FakeService()),serviceType, serviceType, ServiceLifetime.Singleton },                    
                    { collection => collection.AddSingleton<IFakeService, FakeService>(s => new FakeService()),serviceType, implementationType, ServiceLifetime.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddImplementationFactoryData))]
        public void AddWithFactoryAddsServiceWithRightLifecyle(
            Action<IServiceCollection> addAction,
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifeCycle)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            Assert.Equal(serviceType, descriptor.ServiceType);
            Assert.Equal(implementationType, descriptor.GetImplementationType());
            Assert.Equal(lifeCycle, descriptor.Lifetime);
        }

        public static TheoryData AddInstanceData
        {
            get
            {
                return new TheoryData<Action<IServiceCollection>>
                {
                    { collection => collection.AddInstance<IFakeService>(_instance) },
                    { collection => collection.AddInstance(typeof(IFakeService), _instance) },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddInstanceData))]
        public void AddInstance_AddsWithSingletonLifecycle(Action<IServiceCollection> addAction)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Same(_instance, descriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Theory]
        [MemberData(nameof(AddInstanceData))]
        public void TryAddNoOpFailsIfExists(Action<IServiceCollection> addAction)
        {
            // Arrange
            var collection = new ServiceCollection();
            addAction(collection);

            // Act
            var d = ServiceDescriptor.Transient<IFakeService, FakeService>();

            // Assert
            Assert.False(collection.TryAdd(d));
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Same(_instance, descriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        public static TheoryData TryAddImplementationTypeData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeService);
                return new TheoryData<Func<IServiceCollection, bool>, Type, Type, ServiceLifetime>
                {
                    { collection => collection.TryAddTransient(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Transient },
                    { collection => collection.TryAddTransient<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Transient },
                    { collection => collection.TryAddTransient<IFakeService>(), serviceType, serviceType, ServiceLifetime.Transient },
                    { collection => collection.TryAddTransient(implementationType), implementationType, implementationType, ServiceLifetime.Transient },

                    { collection => collection.TryAddScoped(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Scoped },
                    { collection => collection.TryAddScoped<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Scoped },
                    { collection => collection.TryAddScoped<IFakeService>(), serviceType, serviceType, ServiceLifetime.Scoped },
                    { collection => collection.TryAddScoped(implementationType), implementationType, implementationType, ServiceLifetime.Scoped },

                    { collection => collection.TryAddSingleton(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Singleton },
                    { collection => collection.TryAddSingleton<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Singleton },
                    { collection => collection.TryAddSingleton<IFakeService>(), serviceType, serviceType, ServiceLifetime.Singleton },
                    { collection => collection.TryAddSingleton(implementationType), implementationType, implementationType, ServiceLifetime.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TryAddImplementationTypeData))]
        public void TryAdd_WithType_AddsService(
            Func<IServiceCollection, bool> addAction,
            Type expectedServiceType,
            Type expectedImplementationType,
            ServiceLifetime expectedLifetime)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            var result = addAction(collection);

            // Assert
            Assert.True(result);

            var descriptor = Assert.Single(collection);
            Assert.Equal(expectedServiceType, descriptor.ServiceType);
            Assert.Same(expectedImplementationType, descriptor.ImplementationType);
            Assert.Equal(expectedLifetime, descriptor.Lifetime);
        }

        [Theory]
        [MemberData(nameof(TryAddImplementationTypeData))]
        public void TryAdd_WithType_DoesNotAddDuplicate(
            Func<IServiceCollection, bool> addAction,
            Type expectedServiceType,
            Type expectedImplementationType,
            ServiceLifetime expectedLifetime)
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.Add(ServiceDescriptor.Transient(expectedServiceType, expectedServiceType));

            // Act
            var result = addAction(collection);

            // Assert
            Assert.False(result);

            var descriptor = Assert.Single(collection);
            Assert.Equal(expectedServiceType, descriptor.ServiceType);
            Assert.Same(expectedServiceType, descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void TryAddIfMissingActuallyAdds()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            var d = ServiceDescriptor.Transient<IFakeService, FakeService>();

            // Assert
            Assert.True(collection.TryAdd(d));
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Null(descriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void TryAddWithEnumerableReturnsTrueIfAnyAdded()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            var ds = new ServiceDescriptor[] {
                ServiceDescriptor.Transient<IFakeService, FakeService>(),
                ServiceDescriptor.Transient<IFakeService, FakeService>()
            };

            // Assert
            Assert.True(collection.TryAdd(ds));
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Null(descriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void TryAddWithEnumerableReturnsFalseIfNoneAdded()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddSingleton<IFakeService, FakeService>();

            // Act
            var ds = new ServiceDescriptor[] {
                ServiceDescriptor.Transient<IFakeService, FakeService>(),
                ServiceDescriptor.Transient<IFakeService, FakeService>()
            };

            // Assert
            Assert.False(collection.TryAdd(ds));
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Null(descriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        public static TheoryData TryAddEnumerableImplementationTypeData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeService);
                return new TheoryData<ServiceDescriptor, Type, Type, ServiceLifetime>
                {
                    { ServiceDescriptor.Transient<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Transient },
                    { ServiceDescriptor.Transient<IFakeService>(s => new FakeService()), serviceType, serviceType, ServiceLifetime.Transient },
                    { ServiceDescriptor.Transient<IFakeService, FakeService>(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Transient },

                    { ServiceDescriptor.Scoped<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Scoped },
                    { ServiceDescriptor.Scoped<IFakeService>(s => new FakeService()), serviceType, serviceType, ServiceLifetime.Scoped },
                    { ServiceDescriptor.Scoped<IFakeService, FakeService>(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Scoped },

                    { ServiceDescriptor.Singleton<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Singleton },
                    { ServiceDescriptor.Singleton<IFakeService>(s => new FakeService()), serviceType, serviceType, ServiceLifetime.Singleton },
                    { ServiceDescriptor.Singleton<IFakeService,FakeService >(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Singleton },

                    { ServiceDescriptor.Instance<IFakeService>(_instance), serviceType, implementationType, ServiceLifetime.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TryAddEnumerableImplementationTypeData))]
        public void TryAddEnumerable_AddsService(
            ServiceDescriptor descriptor,
            Type expectedServiceType,
            Type expectedImplementationType,
            ServiceLifetime expectedLifetime)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            var result = collection.TryAddEnumerable(descriptor);

            // Assert
            Assert.True(result);

            var d = Assert.Single(collection);
            Assert.Equal(expectedServiceType, d.ServiceType);
            Assert.Equal(expectedImplementationType, d.GetImplementationType());
            Assert.Equal(expectedLifetime, d.Lifetime);
        }


        [Theory]
        [MemberData(nameof(TryAddEnumerableImplementationTypeData))]
        public void TryAddEnumerable_DoesNotAddDuplicate(
            ServiceDescriptor descriptor,
            Type expectedServiceType,
            Type expectedImplementationType,
            ServiceLifetime expectedLifetime)
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.TryAddEnumerable(descriptor);

            // Act
            var result = collection.TryAddEnumerable(descriptor);

            // Assert
            Assert.False(result);

            var d = Assert.Single(collection);
            Assert.Equal(expectedServiceType, d.ServiceType);
            Assert.Equal(expectedImplementationType, d.GetImplementationType());
            Assert.Equal(expectedLifetime, d.Lifetime);
        }

        [Fact]
        public void TryAddEnumerable_WithEnumerable_ReturnsTrueIfAnyAdded()
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            var ds = new ServiceDescriptor[] {
                ServiceDescriptor.Transient<IFakeService, FakeService>(),
                ServiceDescriptor.Transient<IFakeService, FakeService>()
            };

            // Assert
            Assert.True(collection.TryAddEnumerable(ds));
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Null(descriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void TryAddEnumerable_WithEnumerable_ReturnsFalseIfNoneAdded()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddSingleton<IFakeService, FakeService>();

            // Act
            var ds = new ServiceDescriptor[] {
                ServiceDescriptor.Transient<IFakeService, FakeService>(),
                ServiceDescriptor.Transient<IFakeService, FakeService>()
            };

            // Assert
            Assert.False(collection.TryAddEnumerable(ds));
            var descriptor = Assert.Single(collection);
            Assert.Equal(typeof(IFakeService), descriptor.ServiceType);
            Assert.Null(descriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void AddSequence_AddsServicesToCollection()
        {
            // Arrange
            var collection = new ServiceCollection();
            var descriptor1 = new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient);
            var descriptor2 = new ServiceDescriptor(typeof(IFakeOuterService), typeof(FakeOuterService), ServiceLifetime.Transient);
            var descriptors = new[] { descriptor1, descriptor2 };

            // Act
            var result = collection.Add(descriptors);

            // Assert
            Assert.Equal(descriptors, collection);
        }

        [Fact]
        public void Replace_AddsServiceIfServiceTypeIsNotRegistered()
        {
            // Arrange
            var collection = new ServiceCollection();
            var descriptor1 = new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient);
            var descriptor2 = new ServiceDescriptor(typeof(IFakeOuterService), typeof(FakeOuterService), ServiceLifetime.Transient);
            collection.Add(descriptor1);

            // Act
            collection.Replace(descriptor2);

            // Assert
            Assert.Equal(new[] { descriptor1, descriptor2 }, collection);
        }

        [Fact]
        public void Replace_ReplacesFirstServiceWithMatchingServiceType()
        {
            // Arrange
            var collection = new ServiceCollection();
            var descriptor1 = new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient);
            var descriptor2 = new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Transient);
            collection.Add(descriptor1);
            collection.Add(descriptor2);
            var descriptor3 = new ServiceDescriptor(typeof(IFakeService), typeof(FakeService), ServiceLifetime.Singleton);

            // Act
            collection.Replace(descriptor3);

            // Assert
            Assert.Equal(new[] { descriptor2, descriptor3 }, collection);
        }
    }
}