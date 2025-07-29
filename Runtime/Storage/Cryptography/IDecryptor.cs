namespace Raccoons.Storage.Cryptography
{
    public interface IDecryptor
    {
        public byte[] Decrypt(byte[] input);
    }
}
