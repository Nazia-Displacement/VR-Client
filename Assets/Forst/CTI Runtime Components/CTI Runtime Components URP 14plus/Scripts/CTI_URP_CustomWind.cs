using UnityEngine;
using System.Collections;

namespace CTI {

    [ExecuteInEditMode]
	[RequireComponent (typeof (WindZone))]
	public class CTI_URP_CustomWind : MonoBehaviour {

		private WindZone m_WindZone;

		private Vector3 WindDirection;
		private float WindStrength;
		private float WindTurbulence;

	    public float WindMultiplier = 1.0f;

	    private bool init = false;
	    private int CTIWindPID;
        private int CTITurbulencedPID;

        void Init () {
			m_WindZone = GetComponent<WindZone>();
            CTIWindPID = Shader.PropertyToID("_CTI_SRP_Wind");
            CTITurbulencedPID = Shader.PropertyToID("_CTI_SRP_Turbulence");
        }

		void OnValidate () {
			Update ();
		}
		
		void Update () {
			if (!init) {
				Init ();
			}
			WindDirection = transform.forward;

			WindStrength = m_WindZone.windMain;
			WindStrength += m_WindZone.windPulseMagnitude * (1.0f + Mathf.Sin(Time.time * m_WindZone.windPulseFrequency) + 1.0f + Mathf.Sin(Time.time * m_WindZone.windPulseFrequency * 3.0f) ) * 0.5f;
			WindStrength *= WindMultiplier;
			WindTurbulence = m_WindZone.windTurbulence * m_WindZone.windMain * WindMultiplier;

            Shader.SetGlobalVector(CTIWindPID, new Vector4(WindDirection.x, WindDirection.y, WindDirection.z, WindStrength) );
            Shader.SetGlobalFloat(CTITurbulencedPID, WindTurbulence);
            Shader.SetGlobalVector("_ST_WindVector", new Vector4(WindDirection.x, WindDirection.y, WindDirection.z, WindStrength));
            Shader.SetGlobalFloat("_WindQuality", 1); // Sets the wind quality globally
        }
	}
}
