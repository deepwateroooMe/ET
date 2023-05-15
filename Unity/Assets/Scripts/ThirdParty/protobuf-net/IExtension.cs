
using System.IO;
namespace ProtoBuf {
    // Provides addition capability for supporting unexpected fields during
    // protocol-buffer serialization/deserialization. This allows for loss-less
    // round-trip/merge, even when the data is not fully understood.
    public interface IExtension {
        // Requests a stream into which any unexpected fields can be persisted.
        // <returns>A new stream suitable for storing data.</returns>
        Stream BeginAppend();
        // Indicates that all unexpected fields have now been stored. The
        // implementing class is responsible for closing the stream. If
        // "commit" is not true the data may be discarded.
        // <param name="stream">The stream originally obtained by BeginAppend.</param>
        // <param name="commit">True if the append operation completed successfully.</param>
        void EndAppend(Stream stream, bool commit);
        // Requests a stream of the unexpected fields previously stored.
        // <returns>A prepared stream of the unexpected fields.</returns>
        Stream BeginQuery();
        // Indicates that all unexpected fields have now been read. The
        // implementing class is responsible for closing the stream.
        // <param name="stream">The stream originally obtained by BeginQuery.</param>
        void EndQuery(Stream stream);
        // Requests the length of the raw binary stream; this is used
        // when serializing sub-entities to indicate the expected size.
        // <returns>The length of the binary stream representing unexpected data.</returns>
        int GetLength();
    }
    // Provides the ability to remove all existing extension data
    public interface IExtensionResettable : IExtension {
        // Remove all existing extension data
        void Reset();
    }
}
