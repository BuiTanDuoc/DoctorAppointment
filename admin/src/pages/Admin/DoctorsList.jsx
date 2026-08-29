import React, { useContext, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { AdminContext } from '../../context/AdminContext'

const VIEW_STORAGE_KEY = 'doctorListView'

const DoctorsList = () => {

  const { doctors, changeAvailability, aToken, getAllDoctors } = useContext(AdminContext)
  const navigate = useNavigate()

  const [view, setView] = useState(() => localStorage.getItem(VIEW_STORAGE_KEY) || 'card')

  useEffect(() => {
    if (aToken) {
        getAllDoctors()
    }
  }, [aToken])

  const setViewMode = (mode) => {
    setView(mode)
    localStorage.setItem(VIEW_STORAGE_KEY, mode)
  }

  return (
    <div className='m-5 max-h-[90vh] overflow-y-scroll'>
      <div className='flex items-center justify-between'>
        <h1 className='text-lg font-medium'>All Doctors</h1>

        <div className='flex border rounded overflow-hidden text-sm'>
          <button
            onClick={() => setViewMode('card')}
            className={`px-3 py-1.5 flex items-center gap-1.5 transition-colors ${view === 'card' ? 'bg-primary text-white' : 'bg-white text-gray-600 hover:bg-gray-50'}`}
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <rect x="3" y="3" width="7" height="7" rx="1" /><rect x="14" y="3" width="7" height="7" rx="1" />
              <rect x="3" y="14" width="7" height="7" rx="1" /><rect x="14" y="14" width="7" height="7" rx="1" />
            </svg>
            Cards
          </button>
          <button
            onClick={() => setViewMode('list')}
            className={`px-3 py-1.5 flex items-center gap-1.5 border-l transition-colors ${view === 'list' ? 'bg-primary text-white' : 'bg-white text-gray-600 hover:bg-gray-50'}`}
          >
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <line x1="4" y1="6" x2="20" y2="6" /><line x1="4" y1="12" x2="20" y2="12" /><line x1="4" y1="18" x2="20" y2="18" />
            </svg>
            List
          </button>
        </div>
      </div>

      {view === 'card' ? (
        <div className='w-full flex flex-wrap gap-4 pt-5 gap-y-6'>
          {doctors.map((item, index) => (
            <div className='border border-[#C9D8FF] rounded-xl w-56 overflow-hidden group' key={index}>
              {/* Fixed-height frame + object-cover: every photo fills the same
                  box the same way, regardless of its original aspect ratio. */}
              <div className='w-full h-48 overflow-hidden bg-[#EAEFFF] cursor-pointer' onClick={() => navigate(`/edit-doctor/${item._id}`)}>
                <img className='w-full h-full object-cover group-hover:scale-105 transition-all duration-500' src={item.image} alt="" />
              </div>
              <div className='p-4'>
                <p className='text-[#262626] text-lg font-medium truncate'>{item.name}</p>
                <p className='text-[#5C5C5C] text-sm truncate'>{item.speciality}</p>
                <div className='mt-2 flex items-center gap-1 text-sm'>
                  <input onChange={() => changeAvailability(item._id)} type="checkbox" checked={item.available} />
                  <p>Available</p>
                </div>
                <button
                  onClick={() => navigate(`/edit-doctor/${item._id}`)}
                  className='mt-3 w-full border border-primary text-primary text-sm py-1 rounded hover:bg-primary hover:text-white transition-all'
                >
                  Edit
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className='w-full pt-5'>
          <div className='border rounded-lg overflow-hidden'>
            <div className='hidden sm:grid grid-cols-[56px_1.5fr_1fr_0.8fr_0.8fr_80px] gap-3 px-4 py-2 bg-[#F2F3FF] text-xs font-medium text-gray-500 uppercase'>
              <span></span>
              <span>Name</span>
              <span>Speciality</span>
              <span>Fees</span>
              <span>Available</span>
              <span></span>
            </div>
            {doctors.map((item, index) => (
              <div
                key={index}
                className='grid grid-cols-[56px_1fr] sm:grid-cols-[56px_1.5fr_1fr_0.8fr_0.8fr_80px] gap-3 items-center px-4 py-2.5 border-t hover:bg-gray-50'
              >
                {/* Same fixed square + object-cover as the card view, just smaller. */}
                <div className='w-10 h-10 rounded-full overflow-hidden bg-[#EAEFFF] cursor-pointer' onClick={() => navigate(`/edit-doctor/${item._id}`)}>
                  <img className='w-full h-full object-cover' src={item.image} alt="" />
                </div>
                <p className='text-[#262626] text-sm font-medium truncate'>{item.name}</p>
                <p className='hidden sm:block text-[#5C5C5C] text-sm truncate'>{item.speciality}</p>
                <p className='hidden sm:block text-sm'>{item.fees}</p>
                <div className='hidden sm:flex items-center gap-1 text-sm'>
                  <input onChange={() => changeAvailability(item._id)} type="checkbox" checked={item.available} />
                </div>
                <button
                  onClick={() => navigate(`/edit-doctor/${item._id}`)}
                  className='justify-self-end text-primary text-sm hover:underline'
                >
                  Edit
                </button>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

export default DoctorsList
