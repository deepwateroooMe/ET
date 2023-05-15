using System;
namespace ProtoBuf {
    // Represents the ability to serialize values to an output of type <typeparamref name="TOutput"/>
    public interface IProtoOutput<TOutput> {
        // Serialize the provided value
        void Serialize<T>(TOutput destination, T value, object userState = null);
    }
    // Represents the ability to serialize values to an output of type <typeparamref name="TOutput"/>
    // with pre-computation of the length
    public interface IMeasuredProtoOutput<TOutput> : IProtoOutput<TOutput> {
        // Measure the length of a value in advance of serialization
        MeasureState<T> Measure<T>(T value, object userState = null);
        // Serialize the previously measured value
        void Serialize<T>(MeasureState<T> measured, TOutput destination);
    }
    // Represents the outcome of computing the length of an object; since this may have required computing lengths
    // for multiple objects, some metadata is retained so that a subsequent serialize operation using
    // this instance can re-use the previously calculated lengths. If the object state changes between the
    // measure and serialize operations, the behavior is undefined.
    public struct MeasureState<T> : IDisposable {
    // note: * does not actually implement this API;
    // it only advertises it for 3.* capability/feature-testing, i.e.
    // callers can check whether a model implements
    // IMeasuredProtoOutput<Foo>, and *work from that*
        // Releases all resources associated with this value
        public void Dispose() => throw new NotImplementedException();
        // Gets the calculated length of this serialize operation, in bytes
        public long Length => throw new NotImplementedException();
    }
}
