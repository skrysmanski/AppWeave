﻿#region License
// Copyright 2021 AppMotor Framework (https://github.com/skrysmanski/AppMotor)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Security.Cryptography.X509Certificates;

using AppMotor.Core.Certificates.Exporting;
using AppMotor.Core.Utils;

using JetBrains.Annotations;

namespace AppMotor.Core.Certificates
{
    /// <summary>
    /// A certificate used for TLS encryption (e.g. HTTPS). To create an instance of this class,
    /// it's first create an instance of <see cref="TlsCertificateSource"/> and then pass it to
    /// the constructors of this class.
    /// </summary>
    /// <remarks>
    /// This class' primary purpose is to make <see cref="X509Certificate2"/> instances easier to
    /// use. To make use of the certificate itself, convert it into a <see cref="X509Certificate2"/>
    /// (an implicit conversion operator is available).
    /// </remarks>
    public sealed class TlsCertificate : Disposable
    {
        private X509Certificate2? _underlyingCertificate;

        /// <summary>
        /// The underlying <see cref="X509Certificate2"/>.
        /// </summary>
        internal X509Certificate2 UnderlyingCertificate => this._underlyingCertificate ?? throw CreateObjectDisposedException();

        /// <summary>
        /// The key algorithm used to create this certificate (used for both public key and private key).
        /// </summary>
        [PublicAPI]
        public CertificateKeyAlgorithms KeyAlgorithm => this.PublicKey.KeyAlgorithm;

        /// <summary>
        /// The subject name of this certificate.
        /// </summary>
        [PublicAPI]
        public X500DistinguishedName SubjectName => this.UnderlyingCertificate.SubjectName;

        /// <summary>
        /// The public key of this certificate.
        /// </summary>
        [PublicAPI]
        public CertificatePublicKey PublicKey { get; }

        /// <summary>
        /// Whether the private key of this certificate is available.
        /// </summary>
        [PublicAPI]
        public bool HasPrivateKey => this.UnderlyingCertificate.HasPrivateKey;

        /// <summary>
        /// <para>Whether private key export is allowed for this certificate. If <c>false</c>,
        /// <see cref="ExportPublicAndPrivateKey"/> will fail.</para>
        /// </summary>
        /// <remarks>
        /// If the private key is not exportable, it is actually "protected" only on Windows.
        /// <see cref="X509Certificate2"/> does not provide export protection on any other
        /// operating system (like Linux). "Export protection" means that an application can
        /// use the private key but it's hidden in a way that it's not possible for the application
        /// to export it.
        /// </remarks>
        /// <seealso cref="HasPrivateKey"/>
        [PublicAPI]
        public bool IsPrivateKeyExportAllowed { get; }

        /// <summary>
        /// This certificate is not valid before this timestamp.
        /// </summary>
        /// <seealso cref="NotAfter"/>
        [PublicAPI]
        public DateTime NotBefore => this.UnderlyingCertificate.NotBefore;

        /// <summary>
        /// This certificate is not valid after this timestamp.
        /// </summary>
        /// <seealso cref="NotBefore"/>
        [PublicAPI]
        public DateTime NotAfter => this.UnderlyingCertificate.NotAfter;

        /// <summary>
        /// The thumbprint of this certificate. This property returns the thumbprint as upper-case hexadecimal string
        /// (e.g. "3A164F12B1D0E208B3FBD94014634A0EBFD0B63B").
        /// </summary>
        [PublicAPI]
        public string Thumbprint => this.UnderlyingCertificate.Thumbprint;

        /// <summary>
        /// Constructor. Private key export is disabled.
        /// </summary>
        /// <param name="certificateSource">The source from which to load the certificate.</param>
        /// <param name="password">The password, if the certificate is encrypted.</param>
        public TlsCertificate(TlsCertificateSource certificateSource, ReadOnlySpan<char> password = default)
            : this(certificateSource, password, allowPrivateKeyExport: false)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="certificateSource">The source from which to load the certificate.</param>
        /// <param name="allowPrivateKeyExport">Whether exporting the private key of the certificate
        /// is allowed. See <see cref="IsPrivateKeyExportAllowed"/> for more details.</param>
        public TlsCertificate(TlsCertificateSource certificateSource, bool allowPrivateKeyExport)
            : this(certificateSource, password: null, allowPrivateKeyExport)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="certificateSource">The source from which to load the certificate.</param>
        /// <param name="allowPrivateKeyExport">Whether exporting the private key of the certificate
        /// is allowed. See <see cref="IsPrivateKeyExportAllowed"/> for more details.</param>
        /// <param name="password">The password, if the certificate is encrypted.</param>
        public TlsCertificate(TlsCertificateSource certificateSource, ReadOnlySpan<char> password, bool allowPrivateKeyExport)
            : this(certificateSource.CreateUnderlyingCertificate(allowPrivateKeyExport, password), allowPrivateKeyExport)
        {
        }

        /// <summary>
        /// Interop constructor. Only use it if you already an instance of <see cref="X509Certificate2"/>.
        /// </summary>
        /// <param name="certificate">The certificate. Note that the created instance assumes ownership of
        /// the certificate.</param>
        /// <param name="allowPrivateKeyExport">Whether the certificate was created with <see cref="X509KeyStorageFlags.Exportable"/>.</param>
        public TlsCertificate(X509Certificate2 certificate, bool allowPrivateKeyExport)
        {
            Validate.ArgumentWithName(nameof(certificate)).IsNotNull(certificate);

            this._underlyingCertificate = certificate;
            this.PublicKey = new CertificatePublicKey(certificate.PublicKey);

            // NOTE: We don't throw an exception here if "allowPrivateKeyExport" is "true" but
            //   the certificate has no private key. Users may not know beforehand whether a
            //   byte blob has a private key or not. Instead we just throw when the user tries
            //   to export the private key.
            this.IsPrivateKeyExportAllowed = allowPrivateKeyExport;
        }

        /// <inheritdoc />
        protected override void DisposeManagedResources()
        {
            base.DisposeManagedResources();

            this._underlyingCertificate?.Dispose();
            this._underlyingCertificate = null;
        }

        /// <summary>
        /// Implicit conversion to <see cref="X509Certificate2"/>.
        /// </summary>
        public static implicit operator X509Certificate2(TlsCertificate source)
        {
            return source.UnderlyingCertificate;
        }

        /// <summary>
        /// Creates a self signed certificate for the specified hostname.
        /// </summary>
        /// <param name="hostname">the host name for the certificate</param>
        /// <param name="certificateLifetime">for how long this certificate is to be valid.</param>
        [PublicAPI, MustUseReturnValue]
        public static TlsCertificate CreateSelfSigned(string hostname, TimeSpan certificateLifetime)
        {
            using var certificateRequest = new TlsCertificateRequest(hostname);

            return certificateRequest.CreateSelfSignedCertificate(certificateLifetime);
        }

        /// <summary>
        /// Allows you to export this certificate's public key into various formats.
        /// </summary>
        /// <returns>An exporter</returns>
        /// <seealso cref="ExportPublicAndPrivateKey"/>
        public TlsCertificatePublicKeyExporter ExportPublicKey()
        {
            return new(this);
        }

        /// <summary>
        /// <para>Allows you to export both the public key and the private key of this certificate into
        /// various formats.</para>
        ///
        /// <para>Note: Private key export must be allowed for this to work (see <see cref="IsPrivateKeyExportAllowed"/>);
        /// otherwise an exception will be thrown.</para>
        /// </summary>
        /// <returns>An exporter</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="IsPrivateKeyExportAllowed"/> is <c>false</c> or
        /// if <see cref="HasPrivateKey"/> is <c>false</c>.</exception>
        /// <seealso cref="ExportPublicKey"/>
        public TlsCertificatePrivateKeyExporter ExportPublicAndPrivateKey()
        {
            if (!this.IsPrivateKeyExportAllowed)
            {
                throw new InvalidOperationException("Exporting the private key of this certificate is not allowed.");
            }

            if (!this.HasPrivateKey)
            {
                throw new InvalidOperationException("This certificate has no private key.");
            }

            return new(this);
        }
    }
}
