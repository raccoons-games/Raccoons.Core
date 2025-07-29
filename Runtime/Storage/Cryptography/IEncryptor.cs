namespace Raccoons.Storage.Cryptography
{
    public interface IEncryptor
    {
        public byte[] Encrypt(byte[] input);
    }
}
