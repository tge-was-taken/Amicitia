namespace AtlusLibSharp.PS2.ELF
{
    /// <summary>
    /// Enumerates the ELF target operating system ABI.
    /// </summary>
    public enum ELFTargetPlatform : byte
    {
        SystemV = 0x00,
        HPUX = 0x01,
        NetBSD = 0x02,
        Linux = 0x03,
        Solaris = 0x06,
        AIX = 0x07,
        IRIX = 0x08,
        FreeBSD = 0x09,
        OpenBSD = 0x0C,
        OpenVMS = 0x0D
    }

    /// <summary>
    /// Enumerates the ELF word size format.
    /// </summary>
    public enum ELFSizeFormat : byte
    {
        ELF32 = 1,
        ELF64 = 2
    }

    /// <summary>
    /// Enumerates the ELF format endianness.
    /// </summary>
    public enum ELFEndianness : byte
    {
        Little = 1,
        Big = 2
    }

    /// <summary>
    /// Enumerates the ELF object type.
    /// </summary>
    public enum ELFType : short
    {
        /// <summary>
        /// No file type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Relocatable file.
        /// </summary>
        Relocatable = 1,

        /// <summary>
        /// Executable file.
        /// </summary>
        Executable = 2,

        /// <summary>
        /// Shared object file.
        /// </summary>
        Shared = 3,

        /// <summary>
        /// Core file.
        /// </summary>
        Core = 4
    }

    /// <summary>
    /// Specifies ELF target platform instruction set architecture.
    /// </summary>
    public enum ELFInstructionSet : short
    {
        NotSpecified = 0x00,
        M32 = 0x01,
        SPARC = 0x02,
        x86 = 0x03,
        M68K = 0x04,
        M88K = 0x05,
        i860 = 0x07,
        MIPS = 0x08,
        PowerPC = 0x14,
        ARM = 0x28,
        SuperH = 0x2A,
        IA_64 = 0x32,
        x86_64 = 0x3E,
        AArch64 = 0xB7
    }

    /// <summary>
    /// Describes the contents and usage of an <see cref="ELFSection"/>.
    /// </summary>
    public enum ELFSectionType : uint
    {
        /// <summary>
        /// This value marks the section as inactive.
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// The section holds information defined by the program, whose format and meaning are
        /// determined solely by the program.
        /// </summary>
        ProgramDefined = 1,

        /// <summary>
        /// The section holds a symbol table. 
        /// </summary>
        SymbolTable = 2,

        /// <summary>
        /// The section holds a string table.
        /// </summary>
        StringTable = 3,

        /// <summary>
        /// The section holds relocation entries with explicit addends.
        /// </summary>
        RelocationAdd = 4,

        /// <summary>
        /// The section holds a symbol hash table. 
        /// All objects participating in dynamic linking must contain a symbol hash table.
        /// </summary>
        SymbolHashTable = 5,

        /// <summary>
        /// The section holds information for dyanmic linking.
        /// </summary>
        Dynamic = 6,

        /// <summary>
        /// The section holds information that marks the file in some way.
        /// </summary>
        Note = 7,

        /// <summary>
        /// The section occupies no space, but it otherwise resembles <see cref="ProgramDefined"/>.
        /// </summary>
        NoBits = 8,

        /// <summary>
        /// The section holds relocation entries without explcit addends.
        /// </summary>
        Relocation = 9,

        /// <summary>
        /// This section type is reserved but has unspecified semantics.
        /// </summary>
        Reserved = 10,

        // SHT_LOPROC 0x70000000
        // SHT_HIPROC 0x7fffffff
        // SHT_LOUSER 0x80000000
        // SHT_HIUSER 0xffffffff
    }
}
