\ mkv tags

\ Copyright (C) 2014 Free Software Foundation, Inc.

\ This file is part of Gforth.

\ Gforth is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation, either version 3
\ of the License, or (at your option) any later version.

\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ You should have received a copy of the GNU General Public License
\ along with this program. If not, see http://www.gnu.org/licenses/.

0 $1A45DFA3 master: EBML
1 $4286 uint: EBMLVersion
1 $42F7 uint: EBMLReadVersion
1 $42F2 uint: EBMLMaxIDLength
1 $42F3 uint: EBMLMaxSizeLength
1 $4282 string: DocType
1 $4287 uint: DocTypeVersion
1 $4285 uint: DocTypeReadVersion
0 :+ $EC binary: Void
0 :+ $BF binary: CRC-32
0 :+ $1B538667 master: SignatureSlot
1 $7E8A uint: SignatureAlgo
1 $7E9A uint: SignatureHash
1 $7EA5 binary: SignaturePublicKey
1 $7EB5 binary: Signature
1 $7E5B master: SignatureElements
2 $7E7B master: SignatureElementList
3 $6532 binary: SignedElement
0 $18538067 master: Segment
1 $114D9B74 master: SeekHead
2 $4DBB master: Seek
3 $53AB binary: SeekID
3 $53AC uint: SeekPosition
1 $1549A966 master: Info
2 $73A4 binary: SegmentUID
2 $7384 utf8: SegmentFilename
2 $3CB923 binary: PrevUID
2 $3C83AB utf8: PrevFilename
2 $3EB923 binary: NextUID
2 $3E83BB utf8: NextFilename
2 $4444 binary: SegmentFamily
2 $6924 master: ChapterTranslate
3 $69FC uint: ChapterTranslateEditionUID
3 $69BF uint: ChapterTranslateCodec
3 $69A5 binary: ChapterTranslateID
2 $2AD7B1 uint: TimecodeScale
2 $4489 float: Duration
2 $4461 date: DateUTC
2 $7BA9 utf8: Title
2 $4D80 utf8: MuxingApp
2 $5741 utf8: WritingApp
1 $1F43B675 master: Cluster
2 $E7 uint: Timecode
2 $5854 master: SilentTracks
3 $58D7 uint: SilentTrackNumber
2 $A7 uint: Position
2 $AB uint: PrevSize
2 $A3 binary: SimpleBlock
2 $A0 master: BlockGroup
3 $A1 binary: Block
3 $A2 binary: BlockVirtual
3 $75A1 master: BlockAdditions
4 $A6 master: BlockMore
5 $EE uint: BlockAddID
5 $A5 binary: BlockAdditional
3 $9B uint: BlockDuration
3 $FA uint: ReferencePriority
3 $FB int: ReferenceBlock
3 $FD int: ReferenceVirtual
3 $A4 binary: CodecState
3 $8E master: Slices
4 $E8 master: TimeSlice
5 $CC uint: LaceNumber
5 $CD uint: FrameNumber
5 $CB uint: BlockAdditionID
5 $CE uint: Delay
5 $CF uint: SliceDuration
3 $C8 master: ReferenceFrame
4 $C9 uint: ReferenceOffset
4 $CA uint: ReferenceTimeCode
2 $AF binary: EncryptedBlock
1 $1654AE6B master: Tracks
2 $AE master: TrackEntry
3 $D7 uint: TrackNumber
3 $73C5 uint: TrackUID
3 $83 uint: TrackType
3 $B9 uint: FlagEnabled
3 $88 uint: FlagDefault
3 $55AA uint: FlagForced
3 $9C uint: FlagLacing
3 $6DE7 uint: MinCache
3 $6DF8 uint: MaxCache
3 $23E383 uint: DefaultDuration
3 $234E7A uint: DefaultDecodedFieldDuration
3 $23314F float: TrackTimecodeScale
3 $537F int: TrackOffset
3 $55EE uint: MaxBlockAdditionID
3 $536E utf8: Name
3 $22B59C string: Language
3 $86 string: CodecID
3 $63A2 binary: CodecPrivate
3 $258688 utf8: CodecName
3 $7446 uint: AttachmentLink
3 $3A9697 utf8: CodecSettings
3 $3B4040 string: CodecInfoURL
3 $26B240 string: CodecDownloadURL
3 $AA uint: CodecDecodeAll
3 $6FAB uint: TrackOverlay
3 $6624 master: TrackTranslate
4 $66FC uint: TrackTranslateEditionUID
4 $66BF uint: TrackTranslateCodec
4 $66A5 binary: TrackTranslateTrackID
3 $E0 master: Video
4 $9A uint: FlagInterlaced
4 $53B8 uint: StereoMode
4 $53C0 uint: AlphaMode
4 $53B9 uint: OldStereoMode
4 $B0 uint: PixelWidth
4 $BA uint: PixelHeight
4 $54AA uint: PixelCropBottom
4 $54BB uint: PixelCropTop
4 $54CC uint: PixelCropLeft
4 $54DD uint: PixelCropRight
4 $54B0 uint: DisplayWidth
4 $54BA uint: DisplayHeight
4 $54B2 uint: DisplayUnit
4 $54B3 uint: AspectRatioType
4 $2EB524 binary: ColourSpace
4 $2FB523 float: GammaValue
4 $2383E3 float: FrameRate
3 $E1 master: Audio
4 $B5 float: SamplingFrequency
4 $78B5 float: OutputSamplingFrequency
4 $9F uint: Channels
4 $7D7B binary: ChannelPositions
4 $6264 uint: BitDepth
3 $E2 master: TrackOperation
4 $E3 master: TrackCombinePlanes
5 $E4 master: TrackPlane
6 $E5 uint: TrackPlaneUID
6 $E6 uint: TrackPlaneType
4 $E9 master: TrackJoinBlocks
5 $ED uint: TrackJoinUID
3 $C0 uint: TrickTrackUID
3 $C1 binary: TrickTrackSegmentUID
3 $C6 uint: TrickTrackFlag
3 $C7 uint: TrickMasterTrackUID
3 $C4 binary: TrickMasterTrackSegmentUID
3 $6D80 master: ContentEncodings
4 $6240 master: ContentEncoding
5 $5031 uint: ContentEncodingOrder
5 $5032 uint: ContentEncodingScope
5 $5033 uint: ContentEncodingType
5 $5034 master: ContentCompression
6 $4254 uint: ContentCompAlgo
6 $4255 binary: ContentCompSettings
5 $5035 master: ContentEncryption
6 $47E1 uint: ContentEncAlgo
6 $47E2 binary: ContentEncKeyID
6 $47E3 binary: ContentSignature
6 $47E4 binary: ContentSigKeyID
6 $47E5 uint: ContentSigAlgo
6 $47E6 uint: ContentSigHashAlgo
1 $1C53BB6B master: Cues
2 $BB master: CuePoint
3 $B3 uint: CueTime
3 $B7 master: CueTrackPositions
4 $F7 uint: CueTrack
4 $F1 uint: CueClusterPosition
4 $F0 uint: CueRelativePosition
4 $B2 uint: CueDuration
4 $5378 uint: CueBlockNumber
4 $EA uint: CueCodecState
4 $DB master: CueReference
5 $96 uint: CueRefTime
5 $97 uint: CueRefCluster
5 $535F uint: CueRefNumber
5 $EB uint: CueRefCodecState
1 $1941A469 master: Attachments
2 $61A7 master: AttachedFile
3 $467E utf8: FileDescription
3 $466E utf8: FileName
3 $4660 string: FileMimeType
3 $465C binary: FileData
3 $46AE uint: FileUID
3 $4675 binary: FileReferral
3 $4661 uint: FileUsedStartTime
3 $4662 uint: FileUsedEndTime
1 $1043A770 master: Chapters
2 $45B9 master: EditionEntry
3 $45BC uint: EditionUID
3 $45BD uint: EditionFlagHidden
3 $45DB uint: EditionFlagDefault
3 $45DD uint: EditionFlagOrdered
3 :+ $B6 master: ChapterAtom
4 $73C4 uint: ChapterUID
4 $5654 utf8: ChapterStringUID
4 $91 uint: ChapterTimeStart
4 $92 uint: ChapterTimeEnd
4 $98 uint: ChapterFlagHidden
4 $4598 uint: ChapterFlagEnabled
4 $6E67 binary: ChapterSegmentUID
4 $6EBC uint: ChapterSegmentEditionUID
4 $63C3 uint: ChapterPhysicalEquiv
4 $8F master: ChapterTrack
5 $89 uint: ChapterTrackNumber
4 $80 master: ChapterDisplay
5 $85 utf8: ChapString
5 $437C string: ChapLanguage
5 $437E string: ChapCountry
4 $6944 master: ChapProcess
5 $6955 uint: ChapProcessCodecID
5 $450D binary: ChapProcessPrivate
5 $6911 master: ChapProcessCommand
6 $6922 uint: ChapProcessTime
6 $6933 binary: ChapProcessData
1 $1254C367 master: Tags
2 $7373 master: Tag
3 $63C0 master: Targets
4 $68CA uint: TargetTypeValue
4 $63CA string: TargetType
4 $63C5 uint: TagTrackUID
4 $63C9 uint: TagEditionUID
4 $63C4 uint: TagChapterUID
4 $63C6 uint: TagAttachmentUID
3 :+ $67C8 master: SimpleTag
4 $45A3 utf8: TagName
4 $447A string: TagLanguage
4 $4484 uint: TagDefault
4 $4487 utf8: TagString
4 $4485 binary: TagBinary
