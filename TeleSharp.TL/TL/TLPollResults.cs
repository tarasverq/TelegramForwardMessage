using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
namespace TeleSharp.TL
{
	[TLObject(-1159937629)]
    public class TLPollResults : TLObject
    {
        public override int Constructor
        {
            get
            {
                return -1159937629;
            }
        }

             public int Flags {get;set;}
     public bool Min {get;set;}
     public TLVector<TLPollAnswerVoters> Results {get;set;}
     public int? TotalVoters {get;set;}
     public TLVector<int> RecentVoters {get;set;}
     public string Solution {get;set;}
     public TLVector<TLAbsMessageEntity> SolutionEntities {get;set;}


		public void ComputeFlags()
		{
			
		}

        public override void DeserializeBody(BinaryReader br)
        {
            Flags = br.ReadInt32();
Min = (Flags & 1) != 0;
if ((Flags & 2) != 0)
Results = (TLVector<TLPollAnswerVoters>)ObjectUtils.DeserializeVector<TLPollAnswerVoters>(br);
else
Results = null;

if ((Flags & 4) != 0)
TotalVoters = br.ReadInt32();
else
TotalVoters = null;

if ((Flags & 8) != 0)
RecentVoters = (TLVector<int>)ObjectUtils.DeserializeVector<int>(br);
else
RecentVoters = null;

if ((Flags & 16) != 0)
Solution = StringUtil.Deserialize(br);
else
Solution = null;

if ((Flags & 16) != 0)
SolutionEntities = (TLVector<TLAbsMessageEntity>)ObjectUtils.DeserializeVector<TLAbsMessageEntity>(br);
else
SolutionEntities = null;


        }

        public override void SerializeBody(BinaryWriter bw)
        {
			bw.Write(Constructor);
            bw.Write(Flags);

if ((Flags & 2) != 0)
ObjectUtils.SerializeObject(Results,bw);
if ((Flags & 4) != 0)
bw.Write(TotalVoters.Value);
if ((Flags & 8) != 0)
ObjectUtils.SerializeObject(RecentVoters,bw);
if ((Flags & 16) != 0)
StringUtil.Serialize(Solution,bw);
if ((Flags & 16) != 0)
ObjectUtils.SerializeObject(SolutionEntities,bw);

        }
    }
}
