using UnityEngine;

namespace dragonBones
{
    public class UnityArmatureDisplay : IArmatureDisplay
    {
        /**
         * @private
         */
        public Armature _armature;

        /**
         * @private
         */
        public GameObject _gameObject = new GameObject();

        /**
         * @private
         */
        public UnityArmatureDisplay()
        {
        }

        /**
         * @inheritDoc
         */
        public void _onClear()
        {
            if (_gameObject)
            {
                Object.Destroy(_gameObject);
            }

            _armature = null;
            _gameObject = null;
        }

        /**
         * @inheritDoc
         */
        public void _dispatchEvent(EventObject eventObject)
        {
        }

        /**
         * @inheritDoc
         */
        public void _debugDraw()
        {
        }

        /**
         * @inheritDoc
         */
        public bool _hasEvent(string type)
        {
            return false;
        }

        /**
         * @inheritDoc
         */
        public void dispose()
        {
            if (_armature != null)
            {
                advanceTimeBySelf(false);
                _armature.dispose();
                _armature = null;
            }
        }

        /**
         * @inheritDoc
         */
        public void advanceTimeBySelf(bool on)
        {
            if (on)
            {
                UnityFactory._clock.add(_armature);
            }
            else
            {
                UnityFactory._clock.remove(_armature);
            }
        }

        /**
         * @inheritDoc
         */
        public Armature armature
        {
            get { return _armature; }
        }

        /**
         * @inheritDoc
         */
        public Animation animation
        {
            get { return _armature.animation; }
        }

        /**
         * @inheritDoc
         */
        public GameObject gameObject
        {
            get { return _gameObject; }
        }
    }
}
