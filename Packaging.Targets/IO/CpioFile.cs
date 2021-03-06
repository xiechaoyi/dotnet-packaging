﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Packaging.Targets.IO
{
    /// <summary>
    /// The cpio archive format collects any number of files, directories, and
    /// other file system objects(symbolic links, device nodes, etc.) into a
    /// single stream of bytes.
    /// </summary>
    /// <remarks>
    /// Each file system object in a cpio archive comprises a header record with
    /// basic numeric metadata followed by the full pathname of the entry and the
    /// file data.The header record stores a series of integer values that generally
    /// follow the fields in struct stat.  (See stat(2) for details.)  The
    /// variants differ primarily in how they store those integers(binary,
    /// octal, or hexadecimal).  The header is followed by the pathname of the
    /// entry(the length of the pathname is stored in the header) and any file
    /// data.The end of the archive is indicated by a special record with the
    /// pathname <c>TRAILER!!!</c>.
    /// </remarks>
    /// <seealso href="https://people.freebsd.org/~kientzle/libarchive/man/cpio.5.txt"/>
    public class CpioFile : IDisposable
    {
        /// <summary>
        /// The <see cref="Stream"/> around which this <see cref="CpioFile"/> wraps.
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// Indicates whether <see cref="stream"/> should be disposed of when this <see cref="CpioFile"/> is disposed of.
        /// </summary>
        private readonly bool leaveOpen;

        /// <summary>
        /// The entry which is currently available.
        /// </summary>
        private CpioHeader entryHeader;

        /// <summary>
        /// The name of the current entry.
        /// </summary>
        private string entryName;

        /// <summary>
        /// The position at which the data for the current entry starts.
        /// </summary>
        private long entryDataOffset;

        /// <summary>
        /// The length of the data for the current entry.
        /// </summary>
        private long entryDataLength;

        /// <summary>
        /// A <see cref="Stream"/> which represents the current entry.
        /// </summary>
        private SubStream entryStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="CpioFile"/> class.
        /// </summary>
        /// <param name="stream">
        /// A <see cref="Stream"/> which represents the CPIO data.
        /// </param>
        /// <param name="leaveOpen">
        /// <see langword="true"/> to leave the underlying <paramref name="stream"/> open when this <see cref="CpioFile"/>
        /// is disposed of; otherwise, <see langword="false"/>.
        /// </param>
        public CpioFile(Stream stream, bool leaveOpen)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.stream = stream;
            this.leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Gets the name of the current entry.
        /// </summary>
        public string EntryName
        {
            get { return this.entryName; }
        }

        /// <summary>
        /// Gets the header of the current entry.
        /// </summary>
        public CpioHeader EntryHeader
        {
            get { return this.entryHeader; }
        }

        /// <summary>
        /// Gets a value indicating whether the current entry is a directory.
        /// </summary>
        public bool IsDirectory
        {
            get { return this.entryDataLength == 0; }
        }

        /// <summary>
        /// Adds an entry to the <see cref="CpioFile"/>
        /// </summary>
        /// <param name="header">
        /// A <see cref="CpioHeader"/> with the item metaata. The <see cref="CpioHeader.Signature"/>,
        /// <see cref="CpioHeader.NameSize"/> and <see cref="CpioHeader.Filesize"/> values are overwritten.
        /// </param>
        /// <param name="name">
        /// The file name of the entry.
        /// </param>
        /// <param name="data">
        /// A <see cref="Stream"/> which contains the file data.
        /// </param>
        public void Write(CpioHeader header, string name, Stream data)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            byte[] nameBytes = Encoding.UTF8.GetBytes(name);

            // We make sure the magic and size fields have the correct values. All other fields
            // are the responsibility of the caller.
            header.Signature = "070701";
            header.NameSize = (uint)(nameBytes.Length + 1);
            header.FileSize = (uint)data.Length;

            this.stream.WriteStruct(header);
            this.stream.Write(nameBytes, 0, nameBytes.Length);
            this.stream.WriteByte(0); //Trailing 0

            // The pathname is followed by NUL bytes so that the total size of the fixed
            // header plus pathname is a multiple of four.
            var headerSize = Marshal.SizeOf<CpioHeader>() + (int)header.NameSize;
            var paddingSize = PaddingSize(headerSize);

            for (int i = 0; i < paddingSize; i++)
            {
                this.stream.WriteByte(0);
            }

            data.Position = 0;
            data.CopyTo(this.stream);

            // The file data is padded to a multiple of four bytes.
            paddingSize = PaddingSize((int)data.Length);

            for (int i = 0; i < paddingSize; i++)
            {
                this.stream.WriteByte(0);
            }
        }

        /// <summary>
        /// Writes the trailer entry.
        /// </summary>
        public void WriteTrailer()
        {
            this.Write(CpioHeader.Empty, "TRAILER!!!", new MemoryStream(Array.Empty<byte>()));
        }

        /// <summary>
        /// Reads the next entry in the <see cref="CpioFile"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if more data is available; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Read()
        {
            if (this.entryStream != null)
            {
                this.entryStream.Dispose();
            }

            if (this.stream.CanSeek)
            {
                // The file data is padded to a multiple of four bytes.
                var nextIndex = this.entryDataOffset + this.entryDataLength;
                nextIndex += PaddingSize((int)this.entryDataLength);
                this.stream.Seek(nextIndex, SeekOrigin.Begin);
            }
            else
            {
                byte[] buffer = new byte[PaddingSize((int)this.entryDataLength)];
                this.stream.Read(buffer, 0, buffer.Length);
            }

            this.entryHeader = this.stream.ReadStruct<CpioHeader>();

            if (this.entryHeader.Signature != "070701")
            {
                throw new InvalidDataException("The magic for the file entry is invalid");
            }

            byte[] nameBytes = new byte[this.entryHeader.NameSize];
            this.stream.Read(nameBytes, 0, nameBytes.Length);

            this.entryName = Encoding.UTF8.GetString(nameBytes, 0, (int)this.entryHeader.NameSize - 1);

            // The pathname is followed by NUL bytes so that the total size of the fixed
            // header plus pathname is a multiple of four.
            var headerSize = Marshal.SizeOf<CpioHeader>() + nameBytes.Length;
            var paddingSize = PaddingSize(headerSize);

            if (nameBytes.Length < paddingSize)
            {
                nameBytes = new byte[paddingSize];
            }

            this.stream.Read(nameBytes, 0, paddingSize);

            this.entryDataOffset = this.stream.Position;
            this.entryDataLength = this.entryHeader.FileSize;
            this.entryStream = new SubStream(this.stream, this.entryDataOffset, this.entryDataLength, leaveParentOpen: true);

            return this.entryName != "TRAILER!!!";
        }

        /// <summary>
        /// Skips the current entry, without reading the entry data.
        /// </summary>
        public void Skip()
        {
            byte[] buffer = new byte[60 * 1024];

            int left = (int)this.entryDataLength;

            while (left > 0)
            {
                int canRead = Math.Min(left, buffer.Length);
                this.stream.Read(buffer, 0, canRead);
                left -= canRead;
            }
        }

        /// <summary>
        /// Returns a <see cref="Stream"/> which represents the content of the current entry in the <see cref="CpioFile"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Stream"/> which represents the content of the current entry in the <see cref="CpioFile"/>.
        /// </returns>
        public Stream Open()
        {
            return this.entryStream;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.leaveOpen)
            {
                this.stream.Dispose();
            }
        }

        public static int PaddingSize(int value)
        {
            if (value % 4 == 0)
            {
                return 0;
            }
            else
            {
                return 4 - value % 4;
            }
        }
    }
}
