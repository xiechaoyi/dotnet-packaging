﻿using System.Collections.Generic;

namespace Packaging.Targets.Rpm
{
    /// <summary>
    /// Specifies the default order in which tags are saved in the various sections. Used mainly to maintain binary compatibility
    /// with some test files.
    /// </summary>
    internal class DefaultOrder
    {
        /// <summary>
        /// Gets the default order in which the tags are saved in the header section.
        /// </summary>
        public static List<IndexTag> Header
        {
            get
            {
                return new List<IndexTag>()
                {
                    IndexTag.RPMTAG_HEADERIMMUTABLE,
                    IndexTag.RPMTAG_HEADERI18NTABLE,
                    IndexTag.RPMTAG_NAME,
                    IndexTag.RPMTAG_VERSION,
                    IndexTag.RPMTAG_RELEASE,
                    IndexTag.RPMTAG_SUMMARY,
                    IndexTag.RPMTAG_DESCRIPTION,
                    IndexTag.RPMTAG_BUILDTIME,
                    IndexTag.RPMTAG_BUILDHOST,
                    IndexTag.RPMTAG_SIZE,
                    IndexTag.RPMTAG_DISTRIBUTION,
                    IndexTag.RPMTAG_VENDOR,
                    IndexTag.RPMTAG_LICENSE,
                    IndexTag.RPMTAG_GROUP,
                    IndexTag.RPMTAG_URL,
                    IndexTag.RPMTAG_OS,
                    IndexTag.RPMTAG_ARCH,
                    IndexTag.RPMTAG_FILESIZES,
                    IndexTag.RPMTAG_FILEMODES,
                    IndexTag.RPMTAG_FILERDEVS,
                    IndexTag.RPMTAG_FILEMTIMES,
                    IndexTag.RPMTAG_FILEDIGESTS,
                    IndexTag.RPMTAG_FILELINKTOS,
                    IndexTag.RPMTAG_FILEFLAGS,
                    IndexTag.RPMTAG_FILEUSERNAME,
                    IndexTag.RPMTAG_FILEGROUPNAME,
                    IndexTag.RPMTAG_SOURCERPM,
                    IndexTag.RPMTAG_FILEVERIFYFLAGS,
                    IndexTag.RPMTAG_PROVIDENAME,
                    IndexTag.RPMTAG_REQUIREFLAGS,
                    IndexTag.RPMTAG_REQUIRENAME,
                    IndexTag.RPMTAG_REQUIREVERSION,
                    IndexTag.RPMTAG_RPMVERSION,
                    IndexTag.RPMTAG_CHANGELOGTIME,
                    IndexTag.RPMTAG_CHANGELOGNAME,
                    IndexTag.RPMTAG_CHANGELOGTEXT,
                    IndexTag.RPMTAG_POSTINPROG,
                    IndexTag.RPMTAG_POSTUNPROG,
                    IndexTag.RPMTAG_COOKIE,
                    IndexTag.RPMTAG_FILEDEVICES,
                    IndexTag.RPMTAG_FILEINODES,
                    IndexTag.RPMTAG_FILELANGS,
                    IndexTag.RPMTAG_PROVIDEFLAGS,
                    IndexTag.RPMTAG_PROVIDEVERSION,
                    IndexTag.RPMTAG_DIRINDEXES,
                    IndexTag.RPMTAG_BASENAMES,
                    IndexTag.RPMTAG_DIRNAMES,
                    IndexTag.RPMTAG_OPTFLAGS,
                    IndexTag.RPMTAG_DISTURL,
                    IndexTag.RPMTAG_PAYLOADFORMAT,
                    IndexTag.RPMTAG_PAYLOADCOMPRESSOR,
                    IndexTag.RPMTAG_PAYLOADFLAGS,
                    IndexTag.RPMTAG_PLATFORM,
                    IndexTag.RPMTAG_FILECOLORS,
                    IndexTag.RPMTAG_FILECLASS,
                    IndexTag.RPMTAG_CLASSDICT,
                    IndexTag.RPMTAG_FILEDEPENDSX,
                    IndexTag.RPMTAG_FILEDEPENDSN,
                    IndexTag.RPMTAG_DEPENDSDICT,
                    IndexTag.RPMTAG_SOURCEPKGID,
                    IndexTag.RPMTAG_FILEDIGESTALGO,
                };
            }
        }

        /// <summary>
        /// Gets the default order in which tags are saved in the signature sectin.
        /// </summary>
        public static List<SignatureTag> Signature
        {
            get
            {
                return new List<SignatureTag>()
                {
                };
            }
        }
    }
}