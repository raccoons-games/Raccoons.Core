# Raccoons Factories

## Creation

Perform any GameObject creation using Factory-pattern. So you don't perform direct instantiate of the prefabs, but delegate it to BaseFactory implementation. So when you want to spawn new object, you can use DI to inject an abstract factory of the object and use it anywhere without knowing the method of object creation, so it is delegated to DI-installer.

Again - reject direct Instantiate! If at some point you want to switch from instantiate to pools, you have to just change some bindings within DI Container.

There are a few ready to use factories to perform GameObject creation.

- InstantiateFactory - performs default instantiate
- ZenjectInstantiateFactory - performs Zenject instantiate using DI container.
- Pool - populates itself with many copies of the prefab to make the creation more performant.

## Initialization

Each Factory does initialization on created object.