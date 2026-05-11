mergeInto(LibraryManager.library, {

  IsMobileDevice: function () {
	return /Android|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
  },

  IsTabActive: function () {
    return !document.hidden;
  },
});