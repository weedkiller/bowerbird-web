﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

using Bowerbird.Core.DesignByContract;
using System;
using Bowerbird.Core.DomainModels.DenormalisedReferences;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using Bowerbird.Core.Events;

namespace Bowerbird.Core.DomainModels
{
    public class MediaResource : DomainModel
    {
        #region Members

        #endregion

        #region Constructors

        protected MediaResource()
            : base()
        {
            InitMembers();

            EnableEvents();
        }

        public MediaResource(
            string mediaType,
            User createdByUser,
            DateTime uploadedOn)
            : base()
        {
            InitMembers();

            MediaType = mediaType;
            UploadedOn = uploadedOn;
            if (createdByUser != null)
            {
                CreatedByUser = createdByUser;
            }

            EnableEvents();
        }

        public MediaResource(
            string mediaType,
            User createdByUser,
            DateTime uploadedOn,
            string key)
            : base()
        {
            InitMembers();

            MediaType = mediaType;
            UploadedOn = uploadedOn;
            Key = key;

            if (createdByUser != null)
            {
                CreatedByUser = createdByUser;
            }

            EnableEvents();
        }

        #endregion

        #region Properties

        public string MediaType { get; private set; }

        public string Key { get; private set; }

        public DenormalisedUserReference CreatedByUser { get; private set; }

        public DateTime UploadedOn { get; private set; }

        public IDictionary<string, MediaResourceFile> Files { get; private set; }

        public IDictionary<string, string> Metadata { get; private set; }

        #endregion

        #region Methods

        private void InitMembers()
        {
            Files = new Dictionary<string, MediaResourceFile>();
            Metadata = new Dictionary<string, string>();
        }

        public MediaResource AddMetadata(string key, string value)
        {
            if (Metadata.ContainsKey(key))
            {
                ((Dictionary<string, string>)Metadata)[key] = value;
            }
            else
            {
                ((Dictionary<string, string>)Metadata).Add(key, value);
            }

            return this;
        }

        private void AddFile(string storedRepresentation, MediaResourceFile file)
        {
            if (Files.ContainsKey(storedRepresentation))
            {
                ((Dictionary<string, MediaResourceFile>)Files).Remove(storedRepresentation);
            }

            ((Dictionary<string, MediaResourceFile>)Files).Add(storedRepresentation, file);
        }

        public MediaResourceFile AddImageFile(string storedRepresentation, string filename, string relativeUri, string format, int width, int height, string extension)
        {
            dynamic file = new MediaResourceFile();

            file.RelativeUri = relativeUri;
            file.Format = format;
            file.Width = width;
            file.Height = height;
            file.Extension = extension;

            AddFile(storedRepresentation, file);

            return file;
        }

        public MediaResourceFile AddVideoFile(
            string storedRepresentation,
            string linkUri,
            string embedText,
            string provider,
            string videoId,
            string width,
            string height
            )
        {
            dynamic file = new MediaResourceFile();

            file.LinkUri = linkUri;
            file.EmbedTag = string.Format(embedText, width, height, videoId);
            file.Provider = provider;
            file.VideoId = videoId;
            file.Width = width;
            file.Height = height;

            AddFile(storedRepresentation, file);

            return file;
        }

        public void FireCreatedEvent(User createdByUser)
        {
            Check.RequireNotNull(createdByUser, "createdByUser");

            FireEvent(new DomainModelCreatedEvent<MediaResource>(this, createdByUser, this));
        }

        #endregion
    }
}