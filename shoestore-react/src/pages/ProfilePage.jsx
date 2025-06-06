import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import { Link } from 'react-router-dom';
import { authService } from '../services/authService';

const ProfilePage = () => {
  const { user, updateUser, isLoading: authLoading } = useAuth();
  const [isEditing, setIsEditing] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  
  const [formData, setFormData] = useState({
    phoneNumber: user?.phoneNumber || '',
    fullName: user?.fullName || '',
    address: user?.address || '',
  });
  
  const [formErrors, setFormErrors] = useState({});

  // Sync formData with user data when user changes
  useEffect(() => {
    if (user && !isEditing) {
      setFormData({
        phoneNumber: user.phoneNumber || '',
        fullName: user.fullName || '',
        address: user.address || '',
      });
    }
  }, [user, isEditing]);

  const validateForm = () => {
    const errors = {};

    if (!formData.phoneNumber) {
      errors.phoneNumber = 'Số điện thoại là bắt buộc';
    } else if (!/^[0-9]{10,11}$/.test(formData.phoneNumber.replace(/\D/g, ''))) {
      errors.phoneNumber = 'Số điện thoại không hợp lệ';
    }

    if (formData.fullName && formData.fullName.length < 2) {
      errors.fullName = 'Họ tên phải có ít nhất 2 ký tự';
    }

    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));

    // Clear specific field error when user starts typing
    if (formErrors[name]) {
      setFormErrors(prev => ({
        ...prev,
        [name]: '',
      }));
    }
  };

  const handleEdit = () => {
    setFormData({
      phoneNumber: user?.phoneNumber || '',
      fullName: user?.fullName || '',
      address: user?.address || '',
    });
    setIsEditing(true);
    setError('');
    setSuccess('');
  };

  const handleCancel = () => {
    setIsEditing(false);
    setFormData({
      phoneNumber: user?.phoneNumber || '',
      fullName: user?.fullName || '',
      address: user?.address || '',
    });
    setFormErrors({});
    setError('');
    setSuccess('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setIsLoading(true);
    setError('');

    try {
      const response = await authService.updateProfile(formData);
      console.log('Update profile response:', response);
      
      if (response.success) {
        // Update the user in context
        if (response.user) {
          console.log('Updating user with API response:', response.user);
          updateUser(response.user);
        } else {
          // If no user data returned, merge with current user
          const updatedUser = {
            ...user,
            ...formData
          };
          console.log('Updating user with merged data:', updatedUser);
          updateUser(updatedUser);
        }
        setIsEditing(false);
        setSuccess('Thông tin cá nhân đã được cập nhật thành công!');
        
        // Clear success message after 3 seconds
        setTimeout(() => {
          setSuccess('');
        }, 3000);
      } else {
        setError(response.message || 'Có lỗi xảy ra khi cập nhật thông tin');
      }
    } catch (err) {
      setError(err.message || 'Có lỗi xảy ra khi cập nhật thông tin');
    } finally {
      setIsLoading(false);
    }
  };

  if (authLoading || !user) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto mb-4"></div>
          <h2 className="text-2xl font-bold text-gray-900">
            Đang tải thông tin...
          </h2>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-12">
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="bg-white shadow rounded-lg">
          {/* Header */}
          <div className="px-6 py-8 border-b border-gray-200">
            <h1 className="text-3xl font-bold text-gray-900">
              Thông tin cá nhân
            </h1>
            <p className="mt-2 text-gray-600">
              Quản lý thông tin tài khoản và cài đặt bảo mật của bạn
            </p>
          </div>

          {/* Profile Info */}
          <div className="px-6 py-8">
            {success && (
              <div className="mb-6 bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded">
                {success}
              </div>
            )}

            {error && (
              <div className="mb-6 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                {error}
              </div>
            )}

            {isEditing ? (
              <form onSubmit={handleSubmit} className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Email (không thể thay đổi)
                    </label>
                    <div className="mt-1 text-sm text-gray-900 bg-gray-100 rounded-md px-3 py-2">
                      {user.email}
                    </div>
                  </div>

                  <div>
                    <label htmlFor="phoneNumber" className="block text-sm font-medium text-gray-700">
                      Số điện thoại *
                    </label>
                    <input
                      type="tel"
                      id="phoneNumber"
                      name="phoneNumber"
                      value={formData.phoneNumber}
                      onChange={handleChange}
                      className={`mt-1 block w-full px-3 py-2 border ${
                        formErrors.phoneNumber ? 'border-red-300' : 'border-gray-300'
                      } rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                      placeholder="Nhập số điện thoại"
                    />
                    {formErrors.phoneNumber && (
                      <p className="mt-1 text-sm text-red-600">{formErrors.phoneNumber}</p>
                    )}
                  </div>

                  <div>
                    <label htmlFor="fullName" className="block text-sm font-medium text-gray-700">
                      Họ và tên
                    </label>
                    <input
                      type="text"
                      id="fullName"
                      name="fullName"
                      value={formData.fullName}
                      onChange={handleChange}
                      className={`mt-1 block w-full px-3 py-2 border ${
                        formErrors.fullName ? 'border-red-300' : 'border-gray-300'
                      } rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                      placeholder="Nhập họ và tên"
                    />
                    {formErrors.fullName && (
                      <p className="mt-1 text-sm text-red-600">{formErrors.fullName}</p>
                    )}
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700">
                      Vai trò (không thể thay đổi)
                    </label>
                    <div className="mt-1">
                      {user.roles && user.roles.length > 0 ? (
                        <div className="flex flex-wrap gap-2">
                          {user.roles.map((role) => (
                            <span
                              key={role}
                              className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800"
                            >
                              {role}
                            </span>
                          ))}
                        </div>
                      ) : (
                        <div className="text-sm text-gray-900 bg-gray-100 rounded-md px-3 py-2">
                          Không có vai trò
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="md:col-span-2">
                    <label htmlFor="address" className="block text-sm font-medium text-gray-700">
                      Địa chỉ
                    </label>
                    <textarea
                      id="address"
                      name="address"
                      rows={3}
                      value={formData.address}
                      onChange={handleChange}
                      className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                      placeholder="Nhập địa chỉ của bạn"
                    />
                  </div>
                </div>

                <div className="flex flex-col sm:flex-row gap-4 pt-4">
                  <button
                    type="submit"
                    disabled={isLoading}
                    className={`inline-flex justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white ${
                      isLoading
                        ? 'bg-gray-400 cursor-not-allowed'
                        : 'bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500'
                    }`}
                  >
                    {isLoading ? (
                      <>
                        <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        Đang lưu...
                      </>
                    ) : (
                      'Lưu thay đổi'
                    )}
                  </button>
                  
                  <button
                    type="button"
                    onClick={handleCancel}
                    className="inline-flex justify-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                  >
                    Hủy bỏ
                  </button>
                </div>
              </form>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Email
                  </label>
                  <div className="mt-1 text-sm text-gray-900 bg-gray-50 rounded-md px-3 py-2">
                    {user.email}
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Số điện thoại
                  </label>
                  <div className="mt-1 text-sm text-gray-900 bg-gray-50 rounded-md px-3 py-2">
                    {user.phoneNumber || 'Chưa cập nhật'}
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Họ và tên
                  </label>
                  <div className="mt-1 text-sm text-gray-900 bg-gray-50 rounded-md px-3 py-2">
                    {user.fullName || 'Chưa cập nhật'}
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700">
                    Vai trò
                  </label>
                  <div className="mt-1">
                    {user.roles && user.roles.length > 0 ? (
                      <div className="flex flex-wrap gap-2">
                        {user.roles.map((role) => (
                          <span
                            key={role}
                            className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800"
                          >
                            {role}
                          </span>
                        ))}
                      </div>
                    ) : (
                      <div className="text-sm text-gray-900 bg-gray-50 rounded-md px-3 py-2">
                        Không có vai trò
                      </div>
                    )}
                  </div>
                </div>

                {user.address && (
                  <div className="md:col-span-2">
                    <label className="block text-sm font-medium text-gray-700">
                      Địa chỉ
                    </label>
                    <div className="mt-1 text-sm text-gray-900 bg-gray-50 rounded-md px-3 py-2">
                      {user.address}
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Actions */}
          {!isEditing && (
            <div className="px-6 py-6 bg-gray-50 border-t border-gray-200 rounded-b-lg">
              <div className="flex flex-col sm:flex-row gap-4">
                <button
                  type="button"
                  onClick={handleEdit}
                  className="inline-flex justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                >
                  Chỉnh sửa thông tin
                </button>
                
                <Link
                  to="/change-password"
                  className="inline-flex justify-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                >
                  Đổi mật khẩu
                </Link>
              </div>
            </div>
          )}
        </div>

        {/* Account Status */}
        <div className="mt-8 bg-white shadow rounded-lg">
          <div className="px-6 py-6">
            <h2 className="text-lg font-medium text-gray-900 mb-4">
              Trạng thái tài khoản
            </h2>
            
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-sm text-gray-700">Tình trạng:</span>
                <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                  user.isLocked
                    ? 'bg-red-100 text-red-800'
                    : 'bg-green-100 text-green-800'
                }`}>
                  {user.isLocked ? 'Bị khóa' : 'Hoạt động'}
                </span>
              </div>
              
              {user.lockoutEnd && (
                <div className="flex items-center justify-between">
                  <span className="text-sm text-gray-700">Khóa đến:</span>
                  <span className="text-sm text-gray-900">
                    {new Date(user.lockoutEnd).toLocaleString('vi-VN')}
                  </span>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfilePage; 